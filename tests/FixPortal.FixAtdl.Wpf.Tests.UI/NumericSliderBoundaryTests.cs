using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AwesomeAssertions;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

public partial class AtdlPanelTests
{
    [Fact]
    public void NumericSlider_CollapsedNativeRangeCanSelectEitherDecimalEndpoint()
    {
        StaTestHarness.Run(() =>
        {
            var slider = new Controls.NumericSlider { Minimum = 9007199254740992m, Maximum = 9007199254740993m };
            var native = Descendants(slider).OfType<Slider>().Single();
            native.Minimum.Should().Be(native.Maximum);
            using var source = new System.Windows.Interop.HwndSource(0, 0, 0, 0, 0, "test", IntPtr.Zero);
            foreach (var key in new[] { Key.End, Key.Home })
            {
                native.RaiseEvent(
                    new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key)
                    {
                        RoutedEvent = UIElement.PreviewKeyDownEvent,
                    }
                );
                slider.Value.Should().Be(key == Key.End ? 9007199254740993m : 9007199254740992m);
            }
        });
    }

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
