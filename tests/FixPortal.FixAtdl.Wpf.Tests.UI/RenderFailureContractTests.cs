using AwesomeAssertions;
using FixPortal.FixAtdl.Diagnostics.Exceptions;
using FixPortal.FixAtdl.Model.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// Audit U2 (merged A40/B15/E1/E2/F12/E7): StrategyPanelRenderer's failure contract is untested - no
/// test references RenderingException, and nothing pins that a XamlParseException from the parse step
/// propagates instead of being swallowed. These tests instantiate the real renderer (no mocks): the
/// two malformed-strategy guards must throw the typed RenderingException with their exact messages,
/// and a custom renderer emitting invalid XAML must surface as a XamlParseException from
/// XamlReader.Parse, unwrapped.
/// </summary>
public class RenderFailureContractTests
{
    [Fact]
    public void RenderWithoutStrategyLayout_ThrowsRenderingException()
    {
        StaTestHarness.Run(() =>
        {
            using var services = new ServiceCollection().BuildServiceProvider();
            var renderer = new Rendering.StrategyPanelRenderer([]);
            var strategy = new FixPortal.FixAtdl.Model.Elements.Strategy_t();

            var render = () => renderer.Render(strategy, services);

            render.Should().Throw<RenderingException>().WithMessage("No strategy layout was supplied.");
        });
    }

    [Fact]
    public void RenderWithoutRootStrategyPanel_ThrowsRenderingException()
    {
        StaTestHarness.Run(() =>
        {
            using var services = new ServiceCollection().BuildServiceProvider();
            var renderer = new Rendering.StrategyPanelRenderer([]);
            var strategy = new FixPortal.FixAtdl.Model.Elements.Strategy_t
            {
                StrategyLayout = new FixPortal.FixAtdl.Model.Elements.StrategyLayout_t { StrategyPanel = null! },
            };

            var render = () => renderer.Render(strategy, services);

            render.Should().Throw<RenderingException>().WithMessage("No strategy panels were found in this strategy.");
        });
    }

    [Fact]
    public void MalformedRendererXaml_PropagatesXamlParseException()
    {
        StaTestHarness.Run(() =>
        {
            using var services = new ServiceCollection()
                .AddFixAtdlWpf()
                .AddTransient<Rendering.IControlRenderer, MalformedSpinnerRenderer>()
                .BuildServiceProvider();
            var renderer = services.GetRequiredService<Rendering.StrategyPanelRenderer>();

            var render = () => renderer.Render(TestStrategies.MinimalOneControlStrategy(), services);

            render.Should().Throw<System.Windows.Markup.XamlParseException>();
        });
    }

    [Fact]
    public void VisitorFallback_NamesTheControlTypeAndId()
    {
        // The Visit(Control_t) arm is where an unrecognised broker control lands, so it is a real
        // diagnostic surface. It threw a bare NotImplementedException documented "should never get
        // called", which told a consumer neither which control failed nor why.
        StaTestHarness.Run(() =>
        {
            using var text = new System.IO.StringWriter();
            using var xml = System.Xml.XmlWriter.Create(text);
            var writer = new Rendering.WpfXmlWriter(xml, []);
            var renderer = new Rendering.WpfControlRenderer(
                writer,
                [],
                new Rendering.DefaultRendering.DefaultNamespaceProvider()
            );
            FixPortal.FixAtdl.Model.Elements.Control_t control = new SingleSpinner_t("theSpinner");

            var visit = () => renderer.Visit(control);

            visit
                .Should()
                .Throw<NotSupportedException>()
                .WithMessage("*SingleSpinner_t*")
                .And.Message.Should()
                .Contain("theSpinner");
        });
    }

    /// <summary>
    /// Emits an element whose Width carries a non-numeric value: well-formed XML, so the writer stage
    /// succeeds, but XamlReader.Parse fails converting the value - the malformed-XAML shape a custom
    /// renderer can realistically produce.
    /// </summary>
    private sealed class MalformedSpinnerRenderer : Rendering.IControlRenderer<SingleSpinner_t>
    {
        public Type ControlType => typeof(SingleSpinner_t);

        public void Render(Rendering.WpfXmlWriter writer, SingleSpinner_t control)
        {
            using (writer.New("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "TextBlock"))
            {
                writer.WriteAttribute(Rendering.WpfXmlWriterAttribute.Width, "not-a-number");
            }
        }
    }
}
