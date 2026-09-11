using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FixPortal.FixAtdl.Wpf.Controls;

public class ClickSelectTextBox : TextBox
{
    public ClickSelectTextBox()
    {
        AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
        AddHandler(GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText), true);
        AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(SelectAllText), true);
        AddHandler(KeyDownEvent, new RoutedEventHandler(HandleHandledKeyDown), true);
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

    private void HandleHandledKeyDown(object sender, RoutedEventArgs e)
    {
        if (e is KeyEventArgs { Key: Key.Up or Key.Down } ke)
        {
            ke.Handled = false;
        }
    }
}
