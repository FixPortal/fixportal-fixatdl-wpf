using CommunityToolkit.Mvvm.ComponentModel;
using FixPortal.FixAtdl.Model.Elements;

namespace FixPortal.FixAtdl.Wpf.Core.ViewModels;

/// <summary>
/// Wraps a single <see cref="ListItem_t"/> option belonging to a <see cref="ListControlViewModel"/>.
/// </summary>
public partial class ListItemViewModel : ObservableObject
{
    private readonly ListControlViewModel _owner;
    private readonly ListItem_t _item;
    private bool _updatingFromOwner;

    internal ListItemViewModel(ListControlViewModel owner, ListItem_t item)
    {
        _owner = owner;
        _item = item;
        _isSelected = owner.IsItemSelected(item.EnumId);
    }

    public string EnumId => _item.EnumId;

    public string UiRep => _item.UiRep;

    public string GroupName => _owner.GroupName;

    public bool IsRequiredParameter => _owner.IsRequiredParameter;

    [ObservableProperty]
    private bool _isSelected;

    partial void OnIsSelectedChanged(bool value)
    {
        if (!_updatingFromOwner)
        {
            _owner.SetItemSelected(EnumId, value);
        }
    }

    /// <summary>
    /// Called by the owning <see cref="ListControlViewModel"/> when a sibling selection change may have
    /// affected this item (single-select controls deselect every other item).
    /// </summary>
    internal void RefreshSelectionFromOwner()
    {
        _updatingFromOwner = true;

        try
        {
            IsSelected = _owner.IsItemSelected(EnumId);
        }
        finally
        {
            _updatingFromOwner = false;
        }
    }
}
