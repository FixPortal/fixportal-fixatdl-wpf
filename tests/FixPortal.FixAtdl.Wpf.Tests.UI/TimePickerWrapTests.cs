using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using AwesomeAssertions;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// Stepping across the day boundary wraps instead of escaping the valid range: 23:59 + 1 hour is
/// 00:59 (not 24:59 or a DateTime-constructor throw), 00:xx - 1 hour is 23:xx, and the minutes
/// rollover lands on zero with the UI text and committed time updated.
/// </summary>
public class TimePickerWrapTests
{
    [Fact]
    public void IncrementHoursAtEndOfDayWrapsToStartOfDay()
    {
        StaTestHarness.Run(() =>
        {
            var picker = new FixPortal.FixAtdl.Wpf.Controls.TimePicker
            {
                Time = new DateTime(1, 1, 1, 23, 59, 0, DateTimeKind.Unspecified),
            };

            ((RepeatButton)picker.FindName("upButton")).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            picker.Time.Should().Be(new DateTime(1, 1, 1, 0, 59, 0, DateTimeKind.Unspecified));
            picker.Hours.Should().Be("00");
            picker.Minutes.Should().Be("59");
        });
    }

    [Fact]
    public void DecrementHoursAtStartOfDayWrapsToEndOfDay()
    {
        StaTestHarness.Run(() =>
        {
            var picker = new FixPortal.FixAtdl.Wpf.Controls.TimePicker
            {
                Time = new DateTime(1, 1, 1, 0, 30, 0, DateTimeKind.Unspecified),
            };

            ((RepeatButton)picker.FindName("downButton")).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            picker.Time.Should().Be(new DateTime(1, 1, 1, 23, 30, 0, DateTimeKind.Unspecified));
            picker.Hours.Should().Be("23");
            picker.Minutes.Should().Be("30");
        });
    }

    [Fact]
    public void IncrementMinutesAtLastMinuteWrapsToZeroAndUpdatesUi()
    {
        StaTestHarness.Run(() =>
        {
            var picker = new FixPortal.FixAtdl.Wpf.Controls.TimePicker
            {
                Time = new DateTime(1, 1, 1, 10, 59, 0, DateTimeKind.Unspecified),
            };
            ((TextBox)picker.FindName("minutes")).RaiseEvent(new RoutedEventArgs(UIElement.GotFocusEvent));

            ((RepeatButton)picker.FindName("upButton")).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            picker.Time.Should().Be(new DateTime(1, 1, 1, 10, 0, 0, DateTimeKind.Unspecified));
            picker.Minutes.Should().Be("00");
        });
    }

    [Fact]
    public void DecrementMinutesAtZeroWrapsToLastMinute()
    {
        StaTestHarness.Run(() =>
        {
            var picker = new FixPortal.FixAtdl.Wpf.Controls.TimePicker
            {
                Time = new DateTime(1, 1, 1, 10, 0, 0, DateTimeKind.Unspecified),
            };
            ((TextBox)picker.FindName("minutes")).RaiseEvent(new RoutedEventArgs(UIElement.GotFocusEvent));

            ((RepeatButton)picker.FindName("downButton")).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            picker.Time.Should().Be(new DateTime(1, 1, 1, 10, 59, 0, DateTimeKind.Unspecified));
            picker.Minutes.Should().Be("59");
        });
    }
}
