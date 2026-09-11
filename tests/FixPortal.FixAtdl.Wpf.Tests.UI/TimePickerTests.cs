using AwesomeAssertions;
using FixPortal.FixAtdl.Wpf.Controls;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// Clearing hours or minutes is a valid empty value. WPF controls run on an STA thread.
/// </summary>
public class TimePickerTests
{
    [Fact]
    public void ClearingHoursOrMinutes_IsTreatedAsValid_OnStaThread()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var picker = new TimePicker { Hours = "10", Minutes = "30" };

                picker.IsContentValid.Should().BeTrue();

                picker.Hours = string.Empty;

                picker.IsContentValid.Should().BeTrue();
                picker.Hours.Should().BeEmpty();

                picker.Minutes = "30";
                picker.Hours = "10";
                picker.Minutes = string.Empty;

                picker.IsContentValid.Should().BeTrue();
                picker.Minutes.Should().BeEmpty();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        thread.Join(TimeSpan.FromSeconds(30)).Should().BeTrue("WPF checks must finish within 30 seconds");

        failure.Should().BeNull();
    }
}
