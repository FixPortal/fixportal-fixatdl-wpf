using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using AwesomeAssertions;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// The four stepping handlers (up/down x hours/minutes) must no-op while the visible content is
/// invalid; otherwise the stale <see cref="FixPortal.FixAtdl.Wpf.Controls.TimeInstant"/> is
/// committed, the invalid text is replaced, and the validity flag flips back to true.
/// </summary>
public class TimePickerInvalidNoOpTests
{
    [Theory]
    [InlineData("upButton", "hours")]
    [InlineData("upButton", "minutes")]
    [InlineData("downButton", "hours")]
    [InlineData("downButton", "minutes")]
    public void SteppingWhileInvalidLeavesContentUnchanged(string buttonName, string focusTarget)
    {
        StaTestHarness.Run(() =>
        {
            var picker = new FixPortal.FixAtdl.Wpf.Controls.TimePicker { Hours = "bad", Minutes = "30" };
            picker.IsContentValid.Should().BeFalse();
            picker.Time.Should().BeNull();

            ((TextBox)picker.FindName(focusTarget)).RaiseEvent(new RoutedEventArgs(UIElement.GotFocusEvent));
            ((RepeatButton)picker.FindName(buttonName)).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            picker.IsContentValid.Should().BeFalse();
            picker.Hours.Should().Be("bad");
            picker.Minutes.Should().Be("30");
            picker.Time.Should().BeNull();
        });
    }
}
