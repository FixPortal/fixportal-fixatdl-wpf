using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Elements.Support;

namespace FixPortal.FixAtdl.Wpf.Core.ViewModels;

/// <summary>
/// Aggregates a <see cref="Strategy_t"/>'s controls as <see cref="ControlViewModel"/>s for editing.
/// </summary>
/// <remarks>
/// <see cref="HasErrors"/> is what Task 6's submit-gating and Task 7's error handling both read.
/// <see cref="Controls"/> is fixed at construction — a strategy's set of controls does not change at
/// runtime, and a mutable collection here would need <c>ErrorsChanged</c> subscriptions wired/unwired on
/// add/remove, which nothing in this layer needs.
/// </remarks>
public partial class EditViewModel : ObservableObject
{
    public EditViewModel(Strategy_t strategy)
    {
        List<ControlViewModel> controls =
        [
            .. strategy.Controls.Select(control => new ControlViewModel(control, ResolveParameter(strategy, control))),
        ];

        foreach (ControlViewModel control in controls)
        {
            control.ErrorsChanged += (_, _) => OnPropertyChanged(nameof(HasErrors));
        }

        Controls = controls;
    }

    public IReadOnlyList<ControlViewModel> Controls { get; }

    public bool HasErrors => Controls.Any(c => c.HasErrors);

    /// <summary>
    /// Reads back edited control values keyed by FIX tag number, for the outbound message (spec data-flow
    /// step 5). Controls with no referenced parameter (no FIX tag) or no value are omitted.
    /// </summary>
    /// <remarks>
    /// Numeric values are formatted with <see cref="CultureInfo.InvariantCulture"/> (matching
    /// <see cref="ControlViewModel"/>'s own invariant-culture convention for numeric conversion) so a
    /// decimal like 12.5 always reads back as "12.5", never a comma-decimal "12,5" that would corrupt the
    /// outbound FIX message on a non-en-US machine.
    /// </remarks>
    public IReadOnlyDictionary<int, string> ReadBackFixValues() =>
        Controls
            .Where(c => c.FixTag is not null && c.Value is not null)
            .ToDictionary(
                c => c.FixTag!.Value,
                c =>
                    c.Value is IFormattable formattable
                        ? formattable.ToString(null, CultureInfo.InvariantCulture)
                        : c.Value!.ToString()!
            );

    private static IParameter? ResolveParameter(Strategy_t strategy, Control_t control)
    {
        return control.ParameterRef is { } parameterRef && strategy.Parameters.Contains(parameterRef)
            ? strategy.Parameters[parameterRef]
            : null;
    }
}
