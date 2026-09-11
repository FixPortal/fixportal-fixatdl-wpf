using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Elements.Support;

namespace FixPortal.FixAtdl.Wpf.Core.ViewModels;

/// <summary>
/// Wraps a single ATDL <see cref="Control_t"/> for MVVM data-binding, exposing <see cref="Value"/> with
/// live <see cref="System.ComponentModel.INotifyDataErrorInfo"/> validation.
/// </summary>
/// <remarks>
/// Validation is delegated to the control's own referenced <see cref="IParameter"/>
/// (<see cref="IParameter.SetValueFromControl"/>), which already implements the full ATDL constraint set
/// (required, min/max, enum membership) per parameter type (see <c>AtdlValueType&lt;T&gt;.ValidateValue</c>).
/// This is a deliberate simplification vs. the original Atdl4net <c>ControlViewModel</c>/
/// <c>InvalidatableControlViewModel</c>: StrategyEdit-driven cross-control validation and StateRule-driven
/// enable/visibility are out of scope for this ViewModel layer and remain the Strategy_t-level engine's job.
/// </remarks>
public partial class ControlViewModel : ObservableValidator
{
    private readonly Control_t _control;
    private readonly IParameter? _parameter;

    public ControlViewModel(Control_t control, IParameter? parameter = null)
    {
        _control = control;
        _parameter = parameter;
        _value = control.GetCurrentValue();

        // Validate the initial state immediately (rather than waiting for a user edit) so a control that
        // starts out empty and required is already reflected in HasErrors for submit-gating.
        ValidateAllProperties();
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ControlViewModel), nameof(ValidateAgainstAtdlConstraints))]
    private object? _value;

    /// <summary>
    /// Gets the ID of the underlying <see cref="Control_t"/>.
    /// </summary>
    public string Id => _control.Id;

    /// <summary>
    /// Pushes the candidate value into the underlying control, then validates it against the control's
    /// referenced parameter (if any). A control with no <see cref="ParameterRef"/> — and therefore no
    /// resolved parameter — has no ATDL constraints to enforce here and is always valid.
    /// </summary>
    public static ValidationResult ValidateAgainstAtdlConstraints(object? value, ValidationContext context)
    {
        var viewModel = (ControlViewModel)context.ObjectInstance;

        viewModel._control.SetValue(ConvertForControl(value)!);

        if (viewModel._parameter is null)
        {
            return ValidationResult.Success!;
        }

        FixPortal.FixAtdl.Validation.ValidationResult result = viewModel._parameter.SetValueFromControl(
            viewModel._control
        );

        return result.IsValid ? ValidationResult.Success! : new ValidationResult(result.ErrorText);
    }

    // ponytail: only converts numeric literals (int/double/etc.) to decimal for numeric controls whose
    // SetValue only accepts decimal/string/null; a richer per-control-type coercion table can be added if
    // more control kinds need it.
    private static object? ConvertForControl(object? value)
    {
        return value switch
        {
            null or decimal or string => value,
            IConvertible convertible => Convert.ToDecimal(convertible, CultureInfo.InvariantCulture),
            _ => value,
        };
    }
}
