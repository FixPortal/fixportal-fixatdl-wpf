using System.IO;
using System.Windows.Markup;
using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Wpf.Rendering;
using FixPortal.FixAtdl.Xml;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// <see cref="AtdlPanel.Create"/> does two things to the caller's <see cref="Strategy_t"/>: it
/// renders it, which can throw, and it builds an <see cref="Core.ViewModels.EditViewModel"/>, which
/// mutates it by loading init values and applying state rules through the controls. Rendering runs
/// first so a failure leaves the strategy exactly as the caller handed it over.
///
/// The order matters beyond tidiness. A caller that caught a render failure and retried on the same
/// instance previously got a second set of RuleState objects whose restore point was snapshotted
/// from the already-rule-applied value, so a later <c>{NULL}</c> deactivation restored the rule's
/// own value instead of the document's. The documented "use a separate mutable strategy instance
/// for each editor" was the only thing standing against it.
/// </summary>
public class CreateFailureLeavesStrategyUntouchedTests
{
    [Fact]
    public void A_render_failure_leaves_control_values_as_the_caller_supplied_them()
    {
        StaTestHarness.Run(() =>
        {
            Strategy_t strategy = LoadSampleStrategy();

            // Deliberately NOT LoadInitialControlValues: an uninitialized strategy is what makes the
            // view model's mutation visible. Urgency is an enumerated control, so the view model's
            // constructor seeds its EnumState the moment it runs.
            var urgency = strategy.Controls.Single(control => control.Id == "c_urgency");
            urgency
                .GetCurrentValue()
                .Should()
                .BeNull("the fixture must start uninitialized for this to prove anything");

            using var services = new ServiceCollection()
                .AddFixAtdlWpf()
                .AddTransient<IControlRenderer, MalformedSpinnerRenderer>()
                .BuildServiceProvider();

            var create = () => AtdlPanel.Create(strategy, services);

            create.Should().Throw<XamlParseException>("the sample carries a SingleSpinner this renderer breaks");
            urgency
                .GetCurrentValue()
                .Should()
                .BeNull("a failed Create must not leave the caller's strategy half-initialized");
        });
    }

    private static Strategy_t LoadSampleStrategy()
    {
        using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Strategies", "sample-strategy.xml"));
        return new StrategiesReader().Load(stream).Strategies[0];
    }

    /// <summary>
    /// Well-formed XML the writer stage accepts, but a Width that XamlReader.Parse cannot convert -
    /// the failure shape a custom renderer can realistically produce.
    /// </summary>
    private sealed class MalformedSpinnerRenderer : IControlRenderer<SingleSpinner_t>
    {
        public Type ControlType => typeof(SingleSpinner_t);

        public void Render(WpfXmlWriter writer, SingleSpinner_t control)
        {
            using (writer.New("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "TextBlock"))
            {
                writer.WriteAttribute(WpfXmlWriterAttribute.Width, "not-a-number");
            }
        }
    }
}
