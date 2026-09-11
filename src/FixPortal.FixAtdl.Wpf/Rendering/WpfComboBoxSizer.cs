using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering;

// FP Enhancement: RegisterComboBox (the original's per-combo-box width measurement, used only by the
// AutoSizeDropDowns feature) was removed with that feature — see DropDownListRenderer,
// EditableDropDownListRenderer, MultiSelectListRenderer, SingleSelectListRenderer for why. What remains
// is the lookup table surface (Clear/indexer) other code can still query.
public class WpfComboBoxSizer : DependencyObject
{
    private readonly Dictionary<string, double> _desiredSizeTable = [];

    public ComboBox? ExampleComboBox { get; set; }
    public double InitialComboWidth { get; set; }

    public void Clear()
    {
        _desiredSizeTable.Clear();
    }

    public double this[string controlId] => _desiredSizeTable[controlId];
}
