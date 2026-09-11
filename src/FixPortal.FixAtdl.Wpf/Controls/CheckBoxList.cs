using System.Windows;

namespace FixPortal.FixAtdl.Wpf.Controls;

public class CheckBoxList : MultiButtonControlBase
{
    static CheckBoxList()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CheckBoxList),
            new FrameworkPropertyMetadata(typeof(CheckBoxList))
        );
    }
}
