// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System.Windows;
using System.Windows.Input;

namespace FixPortal.FixAtdl.Wpf.Controls;

/// <summary>
/// Represents a DoubleSpinner control for WPF.
/// </summary>
public partial class DoubleSpinner : NumericSpinnerControlBase
{
    private const decimal DefaultInnerIncrement = 1;
    private const decimal DefaultOuterIncrement = 0.01m;

    /// <summary>
    /// Dependency property that provides storage for the InnerIncrement property.
    /// </summary>
    public static readonly DependencyProperty InnerIncrementProperty = DependencyProperty.Register(
        "InnerIncrement",
        typeof(decimal),
        typeof(DoubleSpinner),
        new FrameworkPropertyMetadata(DefaultInnerIncrement)
    );

    /// <summary>
    /// Dependency property that provides storage for the OuterIncrement property.
    /// </summary>
    public static readonly DependencyProperty OuterIncrementProperty = DependencyProperty.Register(
        "OuterIncrement",
        typeof(decimal),
        typeof(DoubleSpinner),
        new FrameworkPropertyMetadata(DefaultOuterIncrement)
    );

    /// <summary>
    /// Initializes a new <see cref="DoubleSpinner"/> instance.
    /// </summary>
    public DoubleSpinner()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets/sets the inner increment.
    /// </summary>
    public decimal InnerIncrement
    {
        get => (decimal)GetValue(InnerIncrementProperty);
        set => SetValue(InnerIncrementProperty, value);
    }

    /// <summary>
    /// Gets/sets the outer increment.
    /// </summary>
    public decimal OuterIncrement
    {
        get => (decimal)GetValue(OuterIncrementProperty);
        set => SetValue(OuterIncrementProperty, value);
    }

    private void InnerDecrementValue() => ChangeValue(InnerIncrement, subtract: true);

    private void InnerIncrementValue() => ChangeValue(InnerIncrement);

    private void OuterDecrementValue() => ChangeValue(OuterIncrement, subtract: true);

    private void OuterIncrementValue() => ChangeValue(OuterIncrement);

    private void ValueKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            InnerDecrementValue();
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            InnerIncrementValue();
            e.Handled = true;
        }
    }

    private void InnerUpButtonClick(object sender, RoutedEventArgs? e)
    {
        InnerIncrementValue();
    }

    private void InnerDownButtonClick(object sender, RoutedEventArgs? e)
    {
        InnerDecrementValue();
    }

    private void InnerUpButtonKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up)
        {
            InnerUpButtonClick(sender, null);

            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            InnerDownButtonClick(sender, null);

            e.Handled = true;
        }
    }

    private void InnerDownButtonKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            InnerDownButtonClick(sender, null);

            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            InnerUpButtonClick(sender, null);

            e.Handled = true;
        }
    }

    private void OuterUpButtonClick(object sender, RoutedEventArgs? e)
    {
        OuterIncrementValue();
    }

    private void OuterDownButtonClick(object sender, RoutedEventArgs? e)
    {
        OuterDecrementValue();
    }

    private void OuterUpButtonKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up)
        {
            OuterUpButtonClick(sender, null);

            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            OuterDownButtonClick(sender, null);

            e.Handled = true;
        }
    }

    private void OuterDownButtonKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            OuterDownButtonClick(sender, null);

            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            OuterUpButtonClick(sender, null);

            e.Handled = true;
        }
    }
}
