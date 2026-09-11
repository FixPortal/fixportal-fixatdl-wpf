using CommunityToolkit.Mvvm.ComponentModel;
using FixPortal.FixAtdl.Diagnostics.Exceptions;
using FixPortal.FixAtdl.Fix;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Controls.Support;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Elements.Support;

namespace FixPortal.FixAtdl.Wpf.Core.ViewModels;

/// <summary>Coordinates control editing, state rules, strategy validation and FIX read-back.</summary>
public class EditViewModel : ObservableObject
{
    private readonly Strategy_t _strategy;
    private readonly List<RuleState> _rules = [];
    private bool _refreshing;
    private bool _refreshPending;

    public EditViewModel(Strategy_t strategy)
    {
        _strategy = strategy;
        var duplicate = strategy
            .Controls.GroupBy(control => control.Id, StringComparer.Ordinal)
            .FirstOrDefault(group => string.IsNullOrEmpty(group.Key) || group.Count() > 1);
        if (duplicate is not null)
        {
            throw new ArgumentException(
                $"Control IDs must be nonempty and unique: '{duplicate.Key}'.",
                nameof(strategy)
            );
        }

        Controls = strategy
            .Controls.Select(control =>
                control is ListControlBase list
                    ? new ListControlViewModel(list, ResolveParameter(strategy, control))
                    : new ControlViewModel(control, ResolveParameter(strategy, control))
            )
            .ToArray();

        foreach (var control in Controls)
        {
            control.UnderlyingControl.StateRules.ResolveAll(strategy);
            _rules.AddRange(control.UnderlyingControl.StateRules.Select(rule => new RuleState(control, rule)));
            control.ErrorsChanged += (_, _) => OnPropertyChanged(nameof(HasErrors));
            control.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName is nameof(ControlViewModel.Value) or nameof(ControlViewModel.IsContentValid))
                {
                    SynchronizeRadioGroup(control);
                    _refreshPending = true;
                    RefreshRules();
                }
            };
        }
        strategy.StrategyEdits.ResolveAll(strategy);
        RefreshRules();
    }

    public IReadOnlyList<ControlViewModel> Controls { get; }
    public IReadOnlyList<string> StrategyErrors { get; private set; } = [];
    public bool HasErrors => Controls.Any(control => control.HasErrors) || StrategyErrors.Count > 0;

    /// <summary>Returns validated parameter wire values. Check HasErrors before submitting.</summary>
    public IReadOnlyDictionary<int, string> ReadBackFixValues()
    {
        if (HasErrors)
        {
            throw new InvalidOperationException("Correct the strategy's validation errors before reading FIX values.");
        }
        return Controls
            .Where(control => control.FixTag is not null && control.WireValue is not null)
            .ToDictionary(control => control.FixTag!.Value, control => control.WireValue!);
    }

    private void SynchronizeRadioGroup(ControlViewModel changed)
    {
        if (
            changed.Value is not true
            || changed.UnderlyingControl is not RadioButton_t { RadioGroup: { Length: > 0 } group }
        )
        {
            return;
        }
        foreach (
            var sibling in Controls.Where(control =>
                control != changed && control.UnderlyingControl is RadioButton_t radio && radio.RadioGroup == group
            )
        )
        {
            sibling.Value = false;
        }
    }

    private void RefreshRules()
    {
        if (_refreshing)
        {
            return;
        }
        _refreshing = true;
        try
        {
            var passes = 0;
            // ponytail: bounded full scans; use a dependency queue if very large strategies need it.
            do
            {
                _refreshPending = false;
                foreach (var rule in _rules)
                {
                    rule.Apply();
                }
                if (++passes > Math.Max(64, _rules.Count * 4))
                {
                    StrategyErrors = ["State rules did not converge. Check for a cyclic value rule."];
                    return;
                }
            } while (_refreshPending);

            _strategy.StrategyEdits.EvaluateAll(FixFieldValueProvider.Empty, false);
            StrategyErrors = _strategy
                .StrategyEdits.Where(edit => !edit.CurrentState)
                .Select(edit => edit.ErrorMessage)
                .ToArray();
        }
        catch (Exception ex)
            when (ex
                    is FixAtdlException
                        or ArgumentException
                        or FormatException
                        or InvalidCastException
                        or OverflowException
            )
        {
            StrategyErrors = [ex.Message];
        }
        finally
        {
            _refreshing = false;
            OnPropertyChanged(nameof(StrategyErrors));
            OnPropertyChanged(nameof(HasErrors));
        }
    }

    private static IParameter? ResolveParameter(Strategy_t strategy, Control_t control) =>
        control.ParameterRef is { } parameterRef && strategy.Parameters.Contains(parameterRef)
            ? strategy.Parameters[parameterRef]
            : null;

    private sealed class RuleState(ControlViewModel control, StateRule_t rule)
    {
        private bool _active;
        private object? _previousValue;

        public void Apply()
        {
            rule.Evaluate();
            var active = rule.CurrentState;
            // A false condition has no initial action; subsequent transitions invert enabled/visible.
            if (active == _active)
            {
                return;
            }
            if (rule.Enabled is { } enabled)
            {
                control.Enabled = active ? enabled : !enabled;
            }
            if (rule.Visible is { } visible)
            {
                control.Visible = active ? visible : !visible;
            }
            if (rule.Value is null)
            {
                _active = active;
                return;
            }

            if (active)
            {
                _previousValue = ControlViewModel.Snapshot(control.Value);
                control.ApplyStateValue(rule.Value);
            }
            else if (rule.Value == "{NULL}")
            {
                control.Value = ControlViewModel.Snapshot(_previousValue);
            }
            _active = active;
        }
    }
}
