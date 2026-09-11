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
    /// Gets the FIX tag number of the control's referenced parameter, or <see langword="null"/> when the
    /// control has no referenced parameter (or the parameter has no FIX tag assigned).
    /// </summary>
    public int? FixTag => _parameter?.FixTag is { } tag ? (int)tag : null;

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

    // Note: only converts numeric literals (int/double/etc.) to decimal for numeric controls whose
    // SetValue only accepts decimal/string/null; a richer per-control-type coercion table can be added if
    // more control kinds need it. A value that fails decimal conversion (bool, DateTime, char, etc.) is
    // passed through unchanged so it still reaches SetValue/validation and can fail as a validation error
    // rather than as an unhandled exception from inside the CustomValidation method.
    private static object? ConvertForControl(object? value)
    {
        if (value is null or decimal or string or bool)
        {
            return value;
        }

        if (value is IConvertible convertible)
        {
            try
            {
                return Convert.ToDecimal(convertible, CultureInfo.InvariantCulture);
            }
            catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
            {
                return value;
            }
        }

        return value;
    }
}
