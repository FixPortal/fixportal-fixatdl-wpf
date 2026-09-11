using AwesomeAssertions;
using FixPortal.FixAtdl.Wpf.Controls;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// Regression test for the Gitar PR #4 finding: clearing the Hours/Minutes textbox must be
/// treated as a valid "no value" state, not an invalid parse. See ResourceDictionaryLoadTests
/// for why WPF tests run on an explicit STA thread.
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
        thread.Start();
        thread.Join();

        failure.Should().BeNull();
    }
}
