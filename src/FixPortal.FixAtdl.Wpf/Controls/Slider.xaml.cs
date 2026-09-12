using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;

namespace FixPortal.FixAtdl.Wpf.Controls;

/// <summary>
/// Represents a FIXatdl slider control for WPF.
/// </summary>
/// <remarks>This control is implemented using a standard WPF Slider and a series of TextBlocks, positioned
/// using the measuring algorithm in the private UpdateListItems method.  An alternative implementation using
/// WPF UniformGrid was tried, but the custom algorithm was found to give better layout results.</remarks>
public partial class Slider : UserControl, INotifyPropertyChanged
{
    private bool _selectedIndexChangeInProgress = false;
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        "ItemsSource",
        typeof(IList<ListItemViewModel>),
        typeof(Slider),
        new FrameworkPropertyMetadata(OnListItemsChanged)
    );

    public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
        "SelectedValue",
        typeof(string),
        typeof(Slider),
        new FrameworkPropertyMetadata(OnSelectedValueChanged)
    );

    public event PropertyChangedEventHandler? PropertyChanged;

    public Slider()
    {
        InitializeComponent();
    }

    public IList<ListItemViewModel>? ItemsSource
    {
        get => (IList<ListItemViewModel>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string? SelectedValue
    {
        get => (string?)GetValue(SelectedValueProperty);
        set => SetCurrentValue(SelectedValueProperty, value);
    }

    public int SelectedIndex
    {
        get
        {
            if (ItemsSource != null)
            {
                return ItemsSource.ToList().FindIndex(item => item.EnumId == SelectedValue);
            }

            return -1;
        }
        set
        {
            try
            {
                _selectedIndexChangeInProgress = true;

                if (ItemsSource != null && value >= 0 && value < ItemsSource.Count)
                {
                    SelectedValue = ItemsSource[value].EnumId;
                }
                else if (value == -1)
                {
                    SelectedValue = null;
                }
            }
            finally
            {
                _selectedIndexChangeInProgress = false;
            }
        }
    }

    private static void OnListItemsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is IList<ListItemViewModel> items)
        {
            ((Slider)dependencyObject).LayoutControl(items);
        }
    }

    private static void OnSelectedValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        ((Slider)dependencyObject).OnSelectedValueChanged();
    }

    private void OnSelectedValueChanged()
    {
        if (!_selectedIndexChangeInProgress && ItemsSource != null)
        {
            // The source value already changed; only notify the inner slider.

            NotifyPropertyChanged("SelectedIndex");
        }
    }

    private void NotifyPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void LayoutControl(IList<ListItemViewModel> items)
    {
        double desiredWidth = 0;

        var labels = new[] { "Not set" }.Concat(items.Select(item => item.UiRep)).ToArray();
        int numItems = labels.Length;

        Typeface typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        double[] widths = new double[numItems];

        double adjacentWidthDiff = double.MaxValue;

        for (int n = 0; n < numItems; n++)
        {
            FormattedText text = new FormattedText(
                labels[n],
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                FontSize,
                Brushes.Black,
                pixelsPerDip
            );

            widths[n] = text.Width;
            desiredWidth = Math.Max(text.Width, desiredWidth);

            if (n > 0)
            {
                adjacentWidthDiff = Math.Min(adjacentWidthDiff, Math.Abs(widths[n] - widths[n - 1]));
            }
        }

        // The value '10' was determined by trial-and-error, to give a balance between too much gap between
        // adjacent labels, and too little when two adjacent labels are both long.
        double internalMargin = Math.Max(10 - adjacentWidthDiff, 1);

        double spacing = desiredWidth + internalMargin;

        labelArea.Children.Clear();

        for (int n = 0; n < numItems; n++)
        {
            double offset = 0;

            if (n > 0)
            {
                offset = widths[0] / 2 + spacing * n - widths[n] / 2;
            }

            labelArea.Children.Add(new Label() { Content = labels[n], Margin = new Thickness(offset, 0, 0, 0) });
        }

        sliderControl.Width = spacing * Math.Max(0, numItems - 1) + internalMargin + 10;
        sliderControl.Maximum = Math.Max(-1, items.Count - 1);
        NotifyPropertyChanged(nameof(SelectedIndex));
    }
}
