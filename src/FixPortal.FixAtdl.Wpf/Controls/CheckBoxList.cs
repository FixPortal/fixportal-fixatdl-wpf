// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
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
