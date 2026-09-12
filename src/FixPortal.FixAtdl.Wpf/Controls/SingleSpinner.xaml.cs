using System.Windows;
using System.Windows.Input;

namespace FixPortal.FixAtdl.Wpf.Controls;

/// <summary>
/// Represents a SingleSpinner control for WPF.
/// </summary>
public partial class SingleSpinner : NumericSpinnerControlBase
{
    private const decimal DefaultIncrement = 1;

    /// <summary>
    /// Dependency property that provides storage for the Increment property of this control.
    /// </summary>
    public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
        "Increment",
        typeof(decimal),
        typeof(SingleSpinner),
        new FrameworkPropertyMetadata(DefaultIncrement)
    );

    /// <summary>
    /// Initializes a new <see cref="SingleSpinner"/> instance.
    /// </summary>
    public SingleSpinner()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets/sets the Increment property of this control.
    /// </summary>
    public decimal Increment
    {
        get => (decimal)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    private void DecrementValue() => ChangeValue(Increment, subtract: true);

    private void IncrementValue() => ChangeValue(Increment);

    private void upButton_Click(object sender, RoutedEventArgs? e)
    {
        IncrementValue();
    }

    private void downButton_Click(object sender, RoutedEventArgs? e)
    {
        DecrementValue();
    }

    private void value_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            DecrementValue();
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            IncrementValue();
            e.Handled = true;
        }
    }

    private void upButton_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up)
        {
            upButton_Click(sender, null);

            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            downButton_Click(sender, null);

            e.Handled = true;
        }
    }

    private void downButton_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            downButton_Click(sender, null);

            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            upButton_Click(sender, null);

            e.Handled = true;
        }
    }
}
