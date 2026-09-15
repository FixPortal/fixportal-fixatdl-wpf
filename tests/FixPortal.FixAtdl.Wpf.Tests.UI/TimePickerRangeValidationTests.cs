using AwesomeAssertions;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// Parseable-but-out-of-range input (hours 24, minutes 60) must fail clean validation: the
/// content is flagged invalid and preserved, and no exception escapes. Without the 0-23 / 0-59
/// range checks the value reaches the DateTime constructor instead.
/// </summary>
public class TimePickerRangeValidationTests
{
    [Theory]
    [InlineData("Hours", "24", "30")]
    [InlineData("Minutes", "60", "10")]
    public void OutOfRangeValueIsRejectedCleanly(string field, string outOfRange, string inRangeSibling)
    {
        StaTestHarness.Run(() =>
        {
            var picker = new FixPortal.FixAtdl.Wpf.Controls.TimePicker();
            if (field == "Hours")
            {
                picker.Minutes = inRangeSibling;
            }
            else
            {
                picker.Hours = inRangeSibling;
            }

            Action typeOutOfRange =
                field == "Hours" ? () => picker.Hours = outOfRange : () => picker.Minutes = outOfRange;

            typeOutOfRange.Should().NotThrow();
            picker.IsContentValid.Should().BeFalse();
            (field == "Hours" ? picker.Hours : picker.Minutes).Should().Be(outOfRange);
            picker.Time.Should().BeNull();
        });
    }
}
