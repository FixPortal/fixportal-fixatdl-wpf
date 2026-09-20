// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
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
    private bool _synchronizingRadioGroup;

    public EditViewModel(Strategy_t strategy)
        : this(strategy, false) { }

    public EditViewModel(Strategy_t strategy, bool isAmendment)
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

        var duplicateTag = strategy
            .Parameters.Where(parameter => parameter.FixTag is not null)
            .GroupBy(parameter => parameter.FixTag)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateTag is not null)
        {
            // ReadBackFixValues emits one value per FIX tag, so duplicate tags cannot be represented
            // without silently choosing which parameter wins.
            throw new ArgumentException($"Parameter FIX tags must be unique: '{duplicateTag.Key}'.", nameof(strategy));
        }

        Controls = strategy
            .Controls.Select(control =>
                control is ListControlBase list && list is not Slider_t { ListItems.Count: 0 }
                    ? new ListControlViewModel(list, ResolveParameter(strategy, control), isAmendment)
                    : new ControlViewModel(control, ResolveParameter(strategy, control), isAmendment)
            )
            .ToArray();

        ProtectImmutableRadioGroups();

        foreach (var control in Controls)
        {
            control.ParameterValueSource = strategy.Controls.GetParameterValueSource;
            control.Revalidate();
            control.UnderlyingControl.StateRules.ResolveAll(strategy);
            _rules.AddRange(control.UnderlyingControl.StateRules.Select(rule => new RuleState(control, rule)));
            control.ErrorsChanged += (_, _) => OnPropertyChanged(nameof(HasErrors));
            control.PropertyChanged += (_, args) =>
            {
                if (
                    args.PropertyName
                    is nameof(ControlViewModel.Value)
                        or nameof(ControlViewModel.IsContentValid)
                        or nameof(ControlViewModel.HasTornWrite)
                )
                {
                    if (_synchronizingRadioGroup)
                    {
                        return;
                    }
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
    public bool HasErrors =>
        Controls.Any(control => control.HasErrors || control.HasTornWrite) || StrategyErrors.Count > 0;

    /// <summary>Returns validated parameter wire values. Check HasErrors before submitting.</summary>
    public IReadOnlyDictionary<int, string> ReadBackFixValues()
    {
        if (HasErrors)
        {
            throw new InvalidOperationException("Correct the strategy's validation errors before reading FIX values.");
        }
        return _strategy
            .Parameters.Where(parameter => parameter.FixTag is not null && parameter.IsSet)
            .ToDictionary(parameter => checked((int)parameter.FixTag!.Value), parameter => parameter.WireValue!);
    }

    /// <summary>
    /// Returns the FIX StrategyParametersGrp tag sequence (957-960) for hosts using
    /// <see cref="Strategies_t.Tag957Support"/> transport. Check HasErrors before submitting.
    /// </summary>
    public IReadOnlyList<(int Tag, string Value)> ReadBackStrategyParametersGrp()
    {
        if (HasErrors)
        {
            throw new InvalidOperationException("Correct the strategy's validation errors before reading FIX values.");
        }
        return StrategyParametersGrpEmitter.Emit(_strategy);
    }

    private void SynchronizeRadioGroup(ControlViewModel changed)
    {
        if (changed.UnderlyingControl is not RadioButton_t { RadioGroup: { Length: > 0 } group })
        {
            return;
        }
        _synchronizingRadioGroup = true;
        try
        {
            var members = Controls
                .Where(control => control.UnderlyingControl is RadioButton_t radio && radio.RadioGroup == group)
                .ToArray();
            if (changed.Value is true)
            {
                foreach (var sibling in members.Where(control => control != changed))
                {
                    sibling.Value = false;
                }
            }
            foreach (var member in members)
            {
                member.Revalidate();
            }
        }
        finally
        {
            _synchronizingRadioGroup = false;
        }
    }

    private void ProtectImmutableRadioGroups()
    {
        foreach (var control in Controls.Where(control => control.IsReadOnly && control.Value is true))
        {
            if (control.UnderlyingControl is not RadioButton_t { RadioGroup: { Length: > 0 } group })
            {
                continue;
            }
            foreach (
                var sibling in Controls.Where(sibling =>
                    sibling.UnderlyingControl is RadioButton_t radio && radio.RadioGroup == group
                )
            )
            {
                sibling.MakeReadOnly();
            }
        }
    }

    private void RefreshRules()
    {
        if (_refreshing)
        {
            return;
        }
        _refreshing = true;
        var errors = new List<string>();
        var rulesCompleted = false;
        var parameterSweepCompleted = false;
        try
        {
            var passes = 0;
            // ponytail: bounded full scans; use a dependency queue if very large strategies need it.
            do
            {
                _refreshPending = false;
                errors.Clear();
                foreach (var rule in _rules)
                {
                    try
                    {
                        rule.Apply();
                    }
                    catch (Exception ex) when (ControlViewModel.IsValidationException(ex))
                    {
                        errors.Add(ex.Message);
                    }
                }
                if (++passes > Math.Max(64, _rules.Count * 4))
                {
                    errors.Add("State rules did not converge. Check for a cyclic value rule.");
                    break;
                }
            } while (_refreshPending);

            _strategy.StrategyEdits.EvaluateAll(FixFieldValueProvider.Empty, false);
            errors.AddRange(
                _strategy.StrategyEdits.Where(edit => !edit.CurrentState).Select(edit => edit.ErrorMessage)
            );
            rulesCompleted = true;
        }
        catch (Exception ex) when (ControlViewModel.IsValidationException(ex))
        {
            errors.Add(ex.Message);
            rulesCompleted = true;
        }
        finally
        {
            // The inner try/finally is load-bearing: the parameter sweep below can throw anything
            // IsValidationException does not cover (an InternalErrorException from the core, say),
            // and this is the only writer of _refreshing. An escape past the reset would latch the
            // guard at true for the lifetime of this view model, and every later RefreshRules would
            // return at the top - rules silently stop applying while HasErrors still reads clean.
            try
            {
                // Parameters without controls still enforce required, constant and type constraints.
                foreach (
                    var parameter in _strategy.Parameters.Where(parameter =>
                        !Controls.Any(control => control.UnderlyingControl.ParameterRef == parameter.Name)
                    )
                )
                {
                    try
                    {
                        _ = parameter.WireValue;
                    }
                    catch (Exception ex) when (ControlViewModel.IsValidationException(ex))
                    {
                        errors.Add(ex.Message);
                    }
                }
                parameterSweepCompleted = true;
            }
            finally
            {
                if (rulesCompleted && parameterSweepCompleted)
                {
                    StrategyErrors = errors.ToArray();
                }
                _refreshing = false;
                OnPropertyChanged(nameof(StrategyErrors));
                OnPropertyChanged(nameof(HasErrors));
            }
        }
    }

    private static IParameter? ResolveParameter(Strategy_t strategy, Control_t control) =>
        control.ParameterRef is { } parameterRef && strategy.Parameters.Contains(parameterRef)
            ? strategy.Parameters[parameterRef]
            : null;

    private sealed class RuleState(ControlViewModel control, StateRule_t rule)
    {
        private bool? _active;
        private object? _previousValue;

        public void Apply()
        {
            rule.Evaluate();
            var active = rule.CurrentState;
            // Initial false conditions also invert enabled/visible (FIXatdl 1.1 conventions i–ii).
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
                if (rule.Value != "{NULL}" || control.Value is not null)
                {
                    _previousValue = ControlViewModel.Snapshot(control.Value);
                }
                control.ApplyStateValue(rule.Value);
            }
            else if (_active == true && rule.Value == "{NULL}" && _previousValue is not null)
            {
                control.Value = ControlViewModel.Snapshot(_previousValue);
            }
            _active = active;
        }
    }
}
