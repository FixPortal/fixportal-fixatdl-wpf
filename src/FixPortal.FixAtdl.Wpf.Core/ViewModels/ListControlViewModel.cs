using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using FixPortal.FixAtdl.Fix;
using FixPortal.FixAtdl.Model.Controls.Support;
using FixPortal.FixAtdl.Model.Elements.Support;
using FixPortal.FixAtdl.Model.Types.Support;

namespace FixPortal.FixAtdl.Wpf.Core.ViewModels;

/// <summary>
/// Wraps a list-based ATDL control (<see cref="ListControlBase"/> — DropDownList_t, SingleSelectList_t,
/// RadioButtonList_t, etc.) as an <see cref="ObservableCollection{T}"/> of <see cref="ListItemViewModel"/>s
/// plus the currently selected EnumID, with the same ATDL-constraint-driven validation as
/// <see cref="ControlViewModel"/>.
/// </summary>
/// <remarks>
/// Note: single-selection only (matches DropDownList_t/SingleSelectList_t/RadioButtonList_t — the common
/// case). CheckBoxList_t/MultiSelectList_t multi-selection, and EditableDropDownList_t's free-text entry,
/// are not modelled here; add a multi-select variant if/when a control that needs it is wired up.
/// </remarks>
public partial class ListControlViewModel : ObservableValidator
{
    private readonly ListControlBase _control;
    private readonly IParameter? _parameter;

    public ListControlViewModel(ListControlBase control, IParameter? parameter = null)
    {
        _control = control;
        _parameter = parameter;

        // A ListControlBase's EnumState is only populated once LoadInitValue has run; do that eagerly here
        // so the control is usable standalone (mirrors what Strategy_t.LoadInitialControlValues does at
        // strategy-load time).
        control.LoadInitValue(FixFieldValueProvider.Empty);

        Items = new ObservableCollection<ListItemViewModel>(
            control.ListItems.Select(item => new ListItemViewModel(this, item))
        );

        string firstSelected = CurrentState.GetFirstSelectedEnumId();
        _selectedValue = firstSelected.Length == 0 ? null : firstSelected;

        ValidateAllProperties();
    }

    public ObservableCollection<ListItemViewModel> Items { get; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ListControlViewModel), nameof(ValidateSelection))]
    private string? _selectedValue;

    private EnumState CurrentState => (EnumState)_control.GetCurrentValue();

    partial void OnSelectedValueChanged(string? value)
    {
        _ = value; // selection already pushed into the control by ValidateSelection; just sync the items.

        foreach (ListItemViewModel item in Items)
        {
            item.RefreshSelectionFromOwner();
        }
    }

    internal bool IsItemSelected(string enumId) => CurrentState[enumId];

    internal void SetItemSelected(string enumId, bool selected)
    {
        SelectedValue = selected ? enumId : null;
    }

    /// <summary>
    /// Pushes the candidate selection into the underlying control's EnumState, then validates against the
    /// control's referenced parameter (if any). The control update happens here (rather than in
    /// <see cref="OnSelectedValueChanged"/>) because generated validation runs before that partial method,
    /// and validation must see the new selection — same ordering constraint as
    /// <see cref="ControlViewModel.ValidateAgainstAtdlConstraints"/>.
    /// </summary>
    public static ValidationResult ValidateSelection(string? value, ValidationContext context)
    {
        var viewModel = (ListControlViewModel)context.ObjectInstance;

        EnumState newState = viewModel.CurrentState.Copy();

        newState.ClearAll();

        if (value != null)
        {
            newState[value] = true;
        }

        viewModel._control.SetValue(newState);

        if (viewModel._parameter is null)
        {
            return ValidationResult.Success!;
        }

        FixPortal.FixAtdl.Validation.ValidationResult result = viewModel._parameter.SetValueFromControl(
            viewModel._control
        );

        return result.IsValid ? ValidationResult.Success! : new ValidationResult(result.ErrorText);
    }
}
