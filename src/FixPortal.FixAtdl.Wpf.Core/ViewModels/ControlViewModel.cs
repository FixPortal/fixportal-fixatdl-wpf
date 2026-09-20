// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using FixPortal.FixAtdl.Diagnostics.Exceptions;
using FixPortal.FixAtdl.Fix;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Controls.Support;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Elements.Support;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Model.Types;
using FixPortal.FixAtdl.Model.Types.Support;

namespace FixPortal.FixAtdl.Wpf.Core.ViewModels;

/// <summary>Editable control value, presentation state and ATDL parameter validation.</summary>
public partial class ControlViewModel : ObservableValidator
{
    private readonly IParameter? _parameter;

    public ControlViewModel(Control_t control, IParameter? parameter = null)
        : this(control, parameter, false) { }

    public ControlViewModel(Control_t control, IParameter? parameter, bool isAmendment)
    {
        UnderlyingControl = control;
        _parameter = parameter;
        IsReadOnly = isAmendment && parameter?.MutableOnCxlRpl == false;
        // A cleared list retains an EnumState; null means it has never been initialized.
        if (control is ListControlBase and not Slider_t { ListItems.Count: 0 } && control.GetCurrentValue() is null)
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

    /// <summary>
    /// Set when a write escaped validation by exception rather than returning a
    /// <see cref="ValidationResult"/>. The control and its parameter may then disagree, so the
    /// strategy must not be read back as though it were clean. One-way: the model is inconsistent
    /// and only rebuilding it clears that.
    /// </summary>
    private bool _hasTornWrite;

    internal bool HasTornWrite
    {
        get => _hasTornWrite;
        private set => SetProperty(ref _hasTornWrite, value);
    }
    internal Func<Control_t, Control_t>? ParameterValueSource { get; set; }

    internal void Revalidate() => RunValidation(Value);

    private object? _value;

    public bool IsReadOnly { get; private set; }

    internal void MakeReadOnly() => IsReadOnly = true;

    private decimal? DeclaredNumericMinimum =>
        _parameter?.GetValueForControl() switch
        {
            Percentage_t value => value.MinValue * 100,
            Float_t value => value.MinValue,
            Int_t value => value.MinValue,
            NonZeroPositiveIntegerTypeBase => 1,
            _ => null,
        };

    private decimal? DeclaredNumericMaximum =>
        _parameter?.GetValueForControl() switch
        {
            Percentage_t { MaxValue: { } value } => value * 100,
            Float_t { MaxValue: { } value } => value,
            Int_t { MaxValue: { } value } => value,
            _ => null,
        };

    // Native sliders need finite bounds; retain a usable fallback interval when only one is declared.
    public decimal NumericMinimum =>
        DeclaredNumericMinimum
        ?? (DeclaredNumericMaximum is <= 0 and { } maximum ? Math.Max(decimal.MinValue + 100, maximum) - 100 : 0);

    public decimal NumericMaximum =>
        DeclaredNumericMaximum
        ?? (NumericMinimum >= 100 ? Math.Min(decimal.MaxValue - 100, NumericMinimum) + 100 : 100);

    [CustomValidation(typeof(ControlViewModel), nameof(ValidateAgainstAtdlConstraints))]
    public object? Value
    {
        get => _value;
        set
        {
            if (!IsReadOnly && !Equals(_value, value))
            {
                OnPropertyChanging();
                _value = value;
                RunValidation(value);
                OnPropertyChanged();
            }
        }
    }

    private bool _enabled = true;

    public bool Enabled
    {
        get => _enabled && !IsReadOnly;
        set => SetProperty(ref _enabled, value);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Visibility))]
    private bool _visible = true;

    public string Visibility => Visible ? "Visible" : "Collapsed";

    [ObservableProperty]
    private bool _isContentValid = true;

    partial void OnIsContentValidChanged(bool value) => RunValidation(Value);

    private bool _validationWriteStarted;
    private string? _errorToRestore;

    private void RunValidation(object? value)
    {
        _validationWriteStarted = false;
        var previousError = GetErrors(nameof(Value))
            .OfType<ValidationResult>()
            .Select(error => error.ErrorMessage)
            .FirstOrDefault();
        try
        {
            ValidateProperty(value, nameof(Value));
        }
        catch
        {
            if (_validationWriteStarted)
            {
                HasTornWrite = true;
                if (previousError is not null)
                {
                    _errorToRestore = previousError;
                    try
                    {
                        ValidateProperty(value, nameof(Value));
                    }
                    finally
                    {
                        _errorToRestore = null;
                    }
                }
            }
            throw;
        }
        finally
        {
            _validationWriteStarted = false;
        }
    }

    internal static object? Snapshot(object? value) => value is EnumState state ? state.Copy() : value;

    internal void ApplyStateValue(string value)
    {
        if (IsReadOnly)
        {
            return;
        }
        if (
            UnderlyingControl is ListControlBase list
            && list is not Slider_t { ListItems.Count: 0 }
            && value != "{NULL}"
        )
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
        if (model._errorToRestore is { } error)
        {
            return new ValidationResult(error);
        }
        if (!model.IsContentValid)
        {
            return new ValidationResult("Enter a valid value.");
        }
        if (model.UnderlyingControl is TextControlBase && value is not null and not string)
        {
            return new ValidationResult("Enter a text value.");
        }
        if (
            model.UnderlyingControl is ListControlBase and not Slider_t { ListItems.Count: 0 }
            && value is not null and not EnumState
            && !Equals(value, "{NULL}")
        )
        {
            return new ValidationResult("Select a list value.");
        }
        try
        {
            // Loaded display values need not round-trip to the same instant (for example during a DST overlap).
            if (!Equals(value, model.UnderlyingControl.GetCurrentValue()))
            {
                model._validationWriteStarted = true;
                model.UnderlyingControl.SetValue(ConvertForControl(value)!);
            }
            var result = model._parameter?.SetValueFromControl(
                model.ParameterValueSource?.Invoke(model.UnderlyingControl) ?? model.UnderlyingControl
            );
            return result is null || result.IsValid
                ? ValidationResult.Success!
                : new ValidationResult(result.ErrorText);
        }
        catch (Exception ex) when (IsValidationException(ex))
        {
            return new ValidationResult(ex.Message);
        }
    }

    // InternalErrorException is excluded deliberately, and the exclusion has to be explicit: since
    // FixPortal.FixAtdl 1.1.3 it derives from FixAtdlException, so the plain type list below now
    // matches it. It means "the library hit a broken invariant", not "the user typed something
    // wrong" - swallowing it would paint a bug as a field validation error and hide it from the
    // host entirely.
    internal static bool IsValidationException(Exception ex) =>
        ex
            is not InternalErrorException
                and not ArgumentNullException
                and not ArgumentOutOfRangeException
                and (
                    FixAtdlException
                    or ArgumentException
                    or FormatException
                    or InvalidCastException
                    or OverflowException
                );

    private static object? ConvertForControl(object? value) =>
        value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double
            ? Convert.ToDecimal(value, CultureInfo.InvariantCulture)
            : value;
}
