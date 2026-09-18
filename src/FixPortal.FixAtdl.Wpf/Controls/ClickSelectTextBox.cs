// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FixPortal.FixAtdl.Wpf.Controls;

public class ClickSelectTextBox : TextBox
{
    public ClickSelectTextBox()
    {
        // WPF resolves an implicit style by the element's EXACT type, so a TextBox subclass never
        // matches a host's Style TargetType="TextBox" - under Fluent dark, measured, this rendered
        // a white box with black text while a plain TextBox beside it was #0FFFFFFF on #FFFFFFFF.
        // Pointing Style at that resource is the whole fix. It is a DynamicResource, so a host that
        // declares no TextBox style leaves this unset and the control falls back to its theme style
        // exactly as before.
        SetResourceReference(StyleProperty, typeof(TextBox));

        AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
        AddHandler(GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText), true);
        AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(SelectAllText), true);
    }

    private static void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
    {
        DependencyObject? parent = e.OriginalSource as UIElement;

        while (parent != null && parent is not TextBox)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }

        if (parent != null)
        {
            TextBox textBox = (TextBox)parent;

            if (!textBox.IsKeyboardFocusWithin)
            {
                // If the text box is not yet focused, give it the focus and
                // stop further processing of this click event.
                textBox.Focus();

                e.Handled = true;
            }
        }
    }

    private static void SelectAllText(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }
}
