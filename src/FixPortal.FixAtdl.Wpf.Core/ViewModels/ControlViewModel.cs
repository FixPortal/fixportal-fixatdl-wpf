using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using FixPortal.FixAtdl.Diagnostics.Exceptions;
using FixPortal.FixAtdl.Fix;
using FixPortal.FixAtdl.Model.Controls.Support;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Elements.Support;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Model.Types.Support;

namespace FixPortal.FixAtdl.Wpf.Core.ViewModels;

/// <summary>Editable control value, presentation state and ATDL parameter validation.</summary>
public partial class ControlViewModel : ObservableValidator
{
    private readonly IParameter? _parameter;

    public ControlViewModel(Control_t control, IParameter? parameter = null)
    {
        UnderlyingControl = control;
        _parameter = parameter;
        // A cleared list retains an EnumState; null means it has never been initialized.
        if (control is ListControlBase && control.GetCurrentValue() is null)
        {
            control.LoadInitValue(FixFieldValueProvider.Empty);
        }
        _value = Snapshot(control.GetCurrentValue());
        ValidateAllProperties();
    }

    public Control_t UnderlyingControl { get; }
    public string Id => UnderlyingControl.Id;
    public int? FixTag => _parameter?.FixTag is { } tag ? checked((int)tag) : null;
    public string? ToolTip => UnderlyingControl.ToolTip;
    public bool IsRequiredParameter => _parameter?.Use == Use_t.Required;
    internal string? WireValue => HasErrors ? null : _parameter?.WireValue;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ControlViewModel), nameof(ValidateAgainstAtdlConstraints))]
    private object? _value;

    [ObservableProperty]
    private bool _enabled = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Visibility))]
    private bool _visible = true;

    public string Visibility => Visible ? "Visible" : "Collapsed";

    [ObservableProperty]
    private bool _isContentValid = true;

    partial void OnIsContentValidChanged(bool value) => ValidateProperty(Value, nameof(Value));

    internal static object? Snapshot(object? value) => value is EnumState state ? state.Copy() : value;

    internal void ApplyStateValue(string value)
    {
        if (UnderlyingControl is ListControlBase list && value != "{NULL}")
        {
            var state = new EnumState(list.ListItems.EnumIds);
            state.LoadInitValue(value, list is FixPortal.FixAtdl.Model.Controls.EditableDropDownList_t);
            Value = state;
        }
        else
        {
            UnderlyingControl.SetValue(value);
            Value = Snapshot(UnderlyingControl.GetCurrentValue());
        }
    }

    /// <summary>Converts input through the core control and validates its parameter.</summary>
    public static ValidationResult ValidateAgainstAtdlConstraints(object? value, ValidationContext context)
    {
        var model = (ControlViewModel)context.ObjectInstance;
        if (!model.IsContentValid)
        {
            return new ValidationResult("Enter a valid value.");
        }
        try
        {
            // Loaded display values need not round-trip to the same instant (for example during a DST overlap).
            if (!Equals(value, model.UnderlyingControl.GetCurrentValue()))
            {
                model.UnderlyingControl.SetValue(ConvertForControl(value)!);
            }
            var result = model._parameter?.SetValueFromControl(model.UnderlyingControl);
            return result is null || result.IsValid
                ? ValidationResult.Success!
                : new ValidationResult(result.ErrorText);
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
            return new ValidationResult(ex.Message);
        }
    }

    private static object? ConvertForControl(object? value) =>
        value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double
            ? Convert.ToDecimal(value, CultureInfo.InvariantCulture)
            : value;
}
