using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

public partial class AtdlPanelTests
{
    [Theory]
    [InlineData("hours", "minutes", "30", "11", 11, 30)]
    [InlineData("minutes", "hours", "10", "45", 10, 45)]
    public void Clock_ClearingOneFieldPreservesOtherAndBlocksSubmission(
        string cleared,
        string retained,
        string retainedText,
        string replacement,
        int hour,
        int minute
    )
    {
        StaTestHarness.Run(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            // localMktTz is required: a bare time-of-day has no way to become an instant without a
            // zone, and since FixPortal.FixAtdl 1.1.3 the core says so rather than guessing. Every
            // picker edit travels this path, so a Clock_t the WPF adapter can edit must carry one.
            var clock = new Clock_t("Clock") { LocalMktTz = "America/New_York" };
            clock.SetValue(new DateTime(1, 1, 1, 10, 30, 0, DateTimeKind.Unspecified));
            strategy.StrategyLayout.StrategyPanel.Controls.Add(clock);
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(strategy, services);
            Layout(view);
            var picker = Descendants(view).OfType<Controls.TimePicker>().Single();
            var input = (TextBox)picker.FindName(cleared);
            var sibling = (TextBox)picker.FindName(retained);
            input.SetCurrentValue(TextBox.TextProperty, "");
            sibling.Text.Should().Be(retainedText);
            picker.IsContentValid.Should().BeFalse();
            model.HasErrors.Should().BeTrue();
            var read = () => model.ReadBackFixValues();
            read.Should().Throw<InvalidOperationException>();
            input.SetCurrentValue(TextBox.TextProperty, replacement);
            picker.Time.Should().Be(new DateTime(1, 1, 1, hour, minute, 0, DateTimeKind.Unspecified));
            model.HasErrors.Should().BeFalse();
            input.SetCurrentValue(TextBox.TextProperty, "");
            sibling.SetCurrentValue(TextBox.TextProperty, "");
            picker.Time.Should().BeNull();
            model.HasErrors.Should().BeFalse();
            picker.Hours.Should().BeEmpty();
            picker.Minutes.Should().BeEmpty();
            ((RepeatButton)picker.FindName("upButton")).RaiseEvent(
                new System.Windows.RoutedEventArgs(ButtonBase.ClickEvent)
            );
            picker.Hours.Should().Be("01");
            picker.Minutes.Should().Be("00");
            picker.IsContentValid.Should().BeTrue();
        });
    }
}
