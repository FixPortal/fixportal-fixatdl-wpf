using System.Collections.ObjectModel;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Controls.Support;
using FixPortal.FixAtdl.Model.Elements.Support;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Model.Types.Support;

namespace FixPortal.FixAtdl.Wpf.Core.ViewModels;

/// <summary>Single selection, multiple selection and editable list values backed by ATDL enum state.</summary>
public class ListControlViewModel : ControlViewModel
{
    public ListControlViewModel(ListControlBase control, IParameter? parameter = null)
        : base(control, parameter)
    {
        Items = new ObservableCollection<ListItemViewModel>(
            control.ListItems.Select(item => new ListItemViewModel(this, item))
        );
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Value))
            {
                OnPropertyChanged(nameof(SelectedValue));
                OnPropertyChanged(nameof(Text));
                foreach (var item in Items)
                {
                    item.RefreshSelectionFromOwner();
                }
            }
        };
    }

    public ObservableCollection<ListItemViewModel> Items { get; }
    public string GroupName { get; } = Guid.NewGuid().ToString("N");
    public string Orientation =>
        (UnderlyingControl as IOrientableControl)?.Orientation == Orientation_t.Vertical ? "Vertical" : "Horizontal";

    private EnumState CurrentState => (EnumState)UnderlyingControl.GetCurrentValue();

    public string? SelectedValue
    {
        get => CurrentState.GetFirstSelectedEnumId() is { Length: > 0 } id ? id : null;
        set
        {
            if (value == SelectedValue)
            {
                return;
            }
            var state = CurrentState.Copy();
            state.ClearAll();
            if (value is not null)
            {
                state[value] = true;
            }
            Value = state;
        }
    }

    public string? Text
    {
        get => SelectedValue is { } id ? Items.First(item => item.EnumId == id).UiRep : CurrentState.NonEnumValue;
        set
        {
            if (value == Text)
            {
                return;
            }
            var item = Items.FirstOrDefault(item => item.UiRep == value);
            var state = CurrentState.Copy();
            state.ClearAll();
            if (item is not null)
            {
                state[item.EnumId] = true;
            }
            else
            {
                state.NonEnumValue = value;
            }
            Value = state;
        }
    }

    internal bool IsItemSelected(string enumId) => CurrentState[enumId];

    internal void SetItemSelected(string enumId, bool selected)
    {
        var state = CurrentState.Copy();
        if (selected && UnderlyingControl is not (CheckBoxList_t or MultiSelectList_t))
        {
            state.ClearAll();
        }
        state[enumId] = selected;
        Value = state;
    }
}
