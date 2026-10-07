using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FixPortal.FixAtdl.Wpf.Controls;

/// <summary>A native slider with nullable decimal state, preserving absent and loaded values during rendering.</summary>
public sealed class NumericSlider : UserControl
{
    private readonly System.Windows.Controls.Slider _slider = new() { IsSnapToTickEnabled = true, MinWidth = 160 };
    private readonly TextBlock _text = new();
    private bool _updating;

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(decimal?),
        typeof(NumericSlider),
        new FrameworkPropertyMetadata(null, Synchronize)
    );
    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum),
        typeof(decimal),
        typeof(NumericSlider),
        new FrameworkPropertyMetadata(0m, Synchronize)
    );
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum),
        typeof(decimal),
        typeof(NumericSlider),
        new FrameworkPropertyMetadata(100m, Synchronize)
    );
    public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
        nameof(Increment),
        typeof(decimal),
        typeof(NumericSlider),
        new FrameworkPropertyMetadata(1m, Synchronize)
    );

    public NumericSlider()
    {
        var clear = new Button { Content = "Clear", HorizontalAlignment = HorizontalAlignment.Right };
        clear.Click += (_, _) => Value = null;
        var panel = new StackPanel();
        panel.Children.Add(_text);
        panel.Children.Add(_slider);
        panel.Children.Add(clear);
        Content = panel;
        _slider.ValueChanged += (_, args) =>
        {
            if (!_updating)
            {
                Value = (decimal)args.NewValue;
            }
        };
        // Tunneling, so an unset slider takes Minimum before RepeatButton or a key moves the
        // thumb. A release is not a gesture: it also fires when the press began elsewhere,
        // which re-committed a slider a {NULL} rule had just cleared.
        _slider.AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler((_, _) => CommitEmptyValue()));
        _slider.AddHandler(
            PreviewKeyDownEvent,
            new KeyEventHandler(
                (_, args) =>
                {
                    // The device that delivered the key. A modified navigation key does not move the slider.
                    if (args.KeyboardDevice.Modifiers != ModifierKeys.None)
                    {
                        return;
                    }
                    if (
                        args.Key
                        is Key.Left
                            or Key.Right
                            or Key.Up
                            or Key.Down
                            or Key.Home
                            or Key.End
                            or Key.PageUp
                            or Key.PageDown
                    )
                    {
                        CommitEmptyValue();
                    }
                }
            )
        );
        Synchronize();
    }

    public decimal? Value
    {
        get => (decimal?)GetValue(ValueProperty);
        set => SetCurrentValue(ValueProperty, value);
    }

    public decimal Minimum
    {
        get => (decimal)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public decimal Maximum
    {
        get => (decimal)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public decimal Increment
    {
        get => (decimal)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    private static void Synchronize(DependencyObject source, DependencyPropertyChangedEventArgs args) =>
        ((NumericSlider)source).Synchronize();

    private void CommitEmptyValue()
    {
        if (!_updating && Value is null)
        {
            // The slider's double cannot represent every decimal. Casting it overflows within
            // about 4.4e12 of decimal.MaxValue and drops digits past 15 significant figures.
            Value = Minimum;
        }
    }

    private void Synchronize()
    {
        _updating = true;
        try
        {
            _slider.Minimum = (double)Minimum;
            _slider.Maximum = (double)Math.Max(Minimum, Maximum);
            _slider.TickFrequency = (double)Increment;
            _slider.SmallChange = (double)Increment;
            _slider.LargeChange = (double)Increment;
            _slider.Value = (double)(Value ?? Minimum);
            _text.Text = Value?.ToString(CultureInfo.CurrentCulture) ?? "Not set";
        }
        finally
        {
            _updating = false;
        }
    }
}
