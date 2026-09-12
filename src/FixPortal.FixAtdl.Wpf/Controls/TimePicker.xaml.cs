using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FixPortal.FixAtdl.Wpf.Controls;

/// <summary>
/// Represents the time picker control which is used to support the FIXatdl Clock_t control type.
/// </summary>
public partial class TimePicker : UserControl, INotifyPropertyChanged
{
    private bool _minutesHasFocus;
    private bool _updatingTime;
    private bool _hoursValid = true;
    private bool _minutesValid = true;
    private string? _hoursText;
    private string? _minutesText;
    private TimeInstant _value = new TimeInstant() { IsEmpty = true };

    /// <summary>
    /// Dependency property that provides storage for this control's Time property.
    /// </summary>
    public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
        "Time",
        typeof(DateTime?),
        typeof(TimePicker),
        new PropertyMetadata(new PropertyChangedCallback(OnTimeChanged))
    );

    /// <summary>
    /// Dependency property that provides storage for the validity state of this control.
    /// </summary>
    public static readonly DependencyProperty IsContentValidProperty = DependencyProperty.Register(
        "IsContentValid",
        typeof(bool),
        typeof(TimePicker),
        new PropertyMetadata(true)
    );

    /// <summary>
    /// Raised whenever a property of interest has changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimePicker"/> control.
    /// </summary>
    public TimePicker()
    {
        InitializeComponent();

        _minutesHasFocus = false;
    }

    /// <summary>
    /// Gets or sets the time.
    /// </summary>
    /// <value>The time.</value>
    public DateTime? Time
    {
        get => (DateTime?)this.GetValue(TimeProperty);
        set => SetCurrentValue(TimeProperty, value);
    }

    /// <summary>
    /// Gets the validity state of this control.
    /// </summary>
    public bool IsContentValid
    {
        get => (bool)GetValue(IsContentValidProperty);
        set => SetCurrentValue(IsContentValidProperty, value);
    }

    /// <summary>
    /// Gets or sets the minutes value. Used for typing in a value for minutes.
    /// </summary>
    /// <value>The minutes.</value>
    public string Minutes
    {
        get => _minutesText ?? (_value.IsEmpty ? string.Empty : _value.Minutes.ToString("D2"));
        set
        {
            _minutesText = value;
            if (string.IsNullOrEmpty(value))
            {
                TimeInstant prevValue = _value;

                _value.IsEmpty = true;
                _hoursText = _minutesText = null;
                _hoursValid = true;
                _minutesValid = true;

                NotifyMinutesPropertyChanged(prevValue, _value);
                NotifyHoursPropertyChanged(prevValue, _value);

                _minutesValid = true;
                UpdateIsContentValid();
            }
            else if (int.TryParse(value, out int parsedMinutes) && parsedMinutes is >= 0 and <= 59)
            {
                TimeInstant prevValue = _value;

                _value.Minutes = parsedMinutes;

                if (_value.IsEmpty)
                {
                    _value.IsEmpty = false;
                    _value.Hours = 0;
                    _hoursText = null;
                    _hoursValid = true;

                    NotifyHoursPropertyChanged(prevValue, _value);
                }

                NotifyMinutesPropertyChanged(prevValue, _value);

                _minutesValid = true;
                UpdateIsContentValid();
            }
            else
            {
                _minutesValid = false;
                UpdateIsContentValid();
            }
        }
    }

    /// <summary>
    /// Gets or sets the hours. Used for typing in a value for hours.
    /// </summary>
    /// <value>The hours.</value>
    public string Hours
    {
        get => _hoursText ?? (_value.IsEmpty ? string.Empty : _value.Hours.ToString("D2"));
        set
        {
            _hoursText = value;
            if (string.IsNullOrEmpty(value))
            {
                TimeInstant prevValue = _value;

                _value.IsEmpty = true;
                _hoursText = _minutesText = null;
                _hoursValid = true;
                _minutesValid = true;

                NotifyMinutesPropertyChanged(prevValue, _value);
                NotifyHoursPropertyChanged(prevValue, _value);

                _hoursValid = true;
                UpdateIsContentValid();
            }
            else if (int.TryParse(value, out int parsedHours) && parsedHours is >= 0 and <= 23)
            {
                TimeInstant prevValue = _value;

                _value.Hours = parsedHours;

                if (_value.IsEmpty)
                {
                    _value.IsEmpty = false;
                    _value.Minutes = 0;
                    _minutesText = null;
                    _minutesValid = true;

                    NotifyMinutesPropertyChanged(prevValue, _value);
                }

                NotifyHoursPropertyChanged(prevValue, _value);

                _hoursValid = true;
                UpdateIsContentValid();
            }
            else
            {
                _hoursValid = false;
                UpdateIsContentValid();
            }
        }
    }

    private static void OnTimeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        ((TimePicker)dependencyObject).OnTimeChanged((DateTime?)e.NewValue);
    }

    private void OnTimeChanged(DateTime? newValue)
    {
        if (_updatingTime)
        {
            return;
        }
        _value.FromDateTime(newValue);
        _hoursText = _minutesText = null;
        _hoursValid = true;
        _minutesValid = true;
        NotifyPropertyChanged(nameof(Hours));
        NotifyPropertyChanged(nameof(Minutes));
        UpdateIsContentValid();
    }

    private void CommitTime()
    {
        _updatingTime = true;
        try
        {
            SetCurrentValue(TimeProperty, _value.ToDateTime());
        }
        finally
        {
            _updatingTime = false;
        }
    }

    private void upButton_Click(object sender, RoutedEventArgs? e)
    {
        if (_minutesHasFocus)
        {
            IncrementMinutes();
        }
        else
        {
            IncrementHours();
        }
    }

    private void hours_GotFocus(object sender, RoutedEventArgs e)
    {
        _minutesHasFocus = false;
    }

    private void minutes_GotFocus(object sender, RoutedEventArgs e)
    {
        _minutesHasFocus = true;
    }

    private void downButton_Click(object sender, RoutedEventArgs? e)
    {
        if (_minutesHasFocus)
        {
            DecrementMinutes();
        }
        else
        {
            DecrementHours();
        }
    }

    private void minutes_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            DecrementMinutes();
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            IncrementMinutes();
            e.Handled = true;
        }
    }

    private void hours_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            DecrementHours();
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            IncrementHours();
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

    private void IncrementHours()
    {
        if (!IsContentValid)
        {
            return;
        }

        _hoursText = null;
        TimeInstant prevValue = _value;

        if (_value.IsEmpty)
        {
            _value = TimeInstant.StartOfDay;

            NotifyMinutesPropertyChanged(prevValue, _value);
        }

        _value.IncrementHours();

        NotifyHoursPropertyChanged(prevValue, _value);
    }

    private void IncrementMinutes()
    {
        if (!IsContentValid)
        {
            return;
        }

        _minutesText = null;
        TimeInstant prevValue = _value;

        if (_value.IsEmpty)
        {
            _value = TimeInstant.StartOfDay;

            NotifyHoursPropertyChanged(prevValue, _value);
        }

        _value.IncrementMinutes();

        NotifyMinutesPropertyChanged(prevValue, _value);
    }

    private void DecrementHours()
    {
        if (!IsContentValid)
        {
            return;
        }

        _hoursText = null;
        TimeInstant prevValue = _value;

        if (_value.IsEmpty)
        {
            _value = TimeInstant.EndOfDay;

            NotifyMinutesPropertyChanged(prevValue, _value);
        }

        _value.DecrementHours();

        NotifyHoursPropertyChanged(prevValue, _value);
    }

    private void DecrementMinutes()
    {
        if (!IsContentValid)
        {
            return;
        }

        _minutesText = null;
        TimeInstant prevValue = _value;

        if (_value.IsEmpty)
        {
            _value = TimeInstant.EndOfDay;

            NotifyHoursPropertyChanged(prevValue, _value);
        }

        _value.DecrementMinutes();

        NotifyMinutesPropertyChanged(prevValue, _value);
    }

    private void NotifyHoursPropertyChanged(TimeInstant oldValue, TimeInstant newValue)
    {
        // Notify when changed, but also when hours is zero, as that is the starting value
        if (TimeInstant.HoursAreDifferent(oldValue, newValue) || newValue.Hours == 0)
        {
            NotifyPropertyChanged("Hours");

            CommitTime();
        }
    }

    private void NotifyMinutesPropertyChanged(TimeInstant oldValue, TimeInstant newValue)
    {
        // Notify when changed, but also when minutes is zero, as that is the starting value
        if (TimeInstant.MinutesAreDifferent(oldValue, newValue) || newValue.Minutes == 0)
        {
            NotifyPropertyChanged("Minutes");

            CommitTime();
        }
    }

    private void UpdateIsContentValid()
    {
        IsContentValid = _hoursValid && _minutesValid;
    }

    private void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
