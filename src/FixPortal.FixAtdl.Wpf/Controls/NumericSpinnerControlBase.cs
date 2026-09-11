using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FixPortal.FixAtdl.Wpf.Controls;

/// <summary>
/// Provides the base class for numeric spinner controls.
/// </summary>
public class NumericSpinnerControlBase : UserControl
{
    private bool _textChangeInProgress;

    /// <summary>
    /// Dependency property that provides storage for the InnerIncrement property.
    /// </summary>
    /// <summary>
    /// Dependency property that provides storage for the output value of this control.  May be null.
    /// </summary>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        "Value",
        typeof(decimal?),
        typeof(NumericSpinnerControlBase),
        new FrameworkPropertyMetadata(OnValuePropertyChanged)
    );

    /// <summary>
    /// Dependency property that provides storage for the current text for this control.  May or may not be a valid number.
    /// </summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        "Text",
        typeof(string),
        typeof(NumericSpinnerControlBase),
        new FrameworkPropertyMetadata(OnTextPropertyChanged)
    );

    /// <summary>
    /// Dependency property that provides storage for the current format provider for this control.  By default, the culture invariant format provider is used.
    /// </summary>
    public static readonly DependencyProperty FormatProviderProperty = DependencyProperty.Register(
        "FormatProvider",
        typeof(IFormatProvider),
        typeof(NumericSpinnerControlBase),
        new PropertyMetadata(CultureInfo.InvariantCulture)
    );

    /// <summary>
    /// Dependency property that provides storage for the validity state of this control.
    /// </summary>
    public static readonly DependencyProperty IsContentValidProperty = DependencyProperty.Register(
        "IsContentValid",
        typeof(bool),
        typeof(NumericSpinnerControlBase),
        new PropertyMetadata(true)
    );

    /// <summary>
    /// Initializes a new <see cref="NumericSpinnerControlBase"/> instance.
    /// </summary>
    public NumericSpinnerControlBase()
    {
        _textChangeInProgress = false;
    }

    /// <summary>
    /// Gets/sets the output value of this control.
    /// </summary>
    public decimal? Value
    {
        get => (decimal?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Gets/sets the format provider for this control.
    /// </summary>
    public IFormatProvider FormatProvider
    {
        get => (IFormatProvider)GetValue(FormatProviderProperty);
        set => SetValue(FormatProviderProperty, value);
    }

    /// <summary>
    /// Gets/sets the text value of this control.
    /// </summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets the validity state of this control.
    /// </summary>
    public bool IsContentValid
    {
        get => (bool)GetValue(IsContentValidProperty);
        set => SetValue(IsContentValidProperty, value);
    }

    private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((NumericSpinnerControlBase)d).OnValuePropertyChanged((decimal?)e.NewValue);
    }

    private void OnValuePropertyChanged(decimal? newValue)
    {
        if (!_textChangeInProgress)
        {
            Text = newValue == null ? string.Empty : ((decimal)newValue).ToString(FormatProvider);
        }
    }

    private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((NumericSpinnerControlBase)d).OnTextPropertyChanged(e.NewValue as string);
    }

    private void OnTextPropertyChanged(string? newValue)
    {
        try
        {
            _textChangeInProgress = true;

            if (string.IsNullOrEmpty(newValue))
            {
                Value = null;

                UpdateIsContentValid(true);
            }
            else
            {
                if (decimal.TryParse(newValue, out decimal decimalValue))
                {
                    Value = decimalValue;

                    UpdateIsContentValid(true);
                }
                else
                {
                    Value = null;

                    UpdateIsContentValid(false);
                }
            }
        }
        finally
        {
            _textChangeInProgress = false;
        }
    }

    private void UpdateIsContentValid(bool value)
    {
        IsContentValid = value;
    }
}
