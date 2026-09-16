// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using System.Windows;

namespace FixPortal.FixAtdl.Wpf.Controls;

/// <summary>
/// Represents a RadioButtonList WPF control.
/// </summary>
public class RadioButtonList : MultiButtonControlBase
{
    /// <summary>
    /// Static constructor for RadioButtonList; overrides the style.
    /// </summary>
    static RadioButtonList()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(RadioButtonList),
            new FrameworkPropertyMetadata(typeof(RadioButtonList))
        );
    }
}
