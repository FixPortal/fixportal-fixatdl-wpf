using System.Globalization;
using System.Windows.Controls;
using AwesomeAssertions;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

public partial class AtdlPanelTests
{
    [Theory]
    [InlineData("0", "79228162514264337593543950335", true)]
    [InlineData("-79228162514264337593543950335", "0", false)]
    [InlineData("0", "12345678901234567890.123456789", true)]
    [InlineData("-12345678901234567890.123456789", "0", false)]
    public void NumericSlider_ThumbReachesExactDeclaredBound(string minimumText, string maximumText, bool upper)
    {
        StaTestHarness.Run(() =>
        {
            var minimum = decimal.Parse(minimumText, CultureInfo.InvariantCulture);
            var maximum = decimal.Parse(maximumText, CultureInfo.InvariantCulture);
            var slider = new Controls.NumericSlider
            {
                Minimum = minimum,
                Maximum = maximum,
                Value = 0m,
            };
            var native = Descendants(slider).OfType<Slider>().Single();
            var move = () => native.Value = upper ? native.Maximum : native.Minimum;

            move.Should().NotThrow();
            slider.Value.Should().Be(upper ? maximum : minimum);
        });
    }
}
