using AwesomeAssertions;
using FixPortal.FixAtdl.Diagnostics.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// Host-composition failure modes (audit 2026-10-11 E9/G3, G2) and venue tooltip text (B8).
/// </summary>
public class RendererCompositionTests
{
    [Fact]
    public void RenderWithoutRendererForControlType_ThrowsRenderingExceptionNamingTheControlType()
    {
        StaTestHarness.Run(() =>
        {
            using var services = new ServiceCollection().BuildServiceProvider();
            var renderer = new Rendering.StrategyPanelRenderer([]);

            var render = () => renderer.Render(TestStrategies.MinimalOneControlStrategy(), services);

            render.Should().Throw<RenderingException>().WithMessage("*SingleSpinner_t*");
        });
    }

    [Fact]
    public void CreateWithoutAddFixAtdlWpf_ThrowsInvalidOperationException()
    {
        StaTestHarness.Run(() =>
        {
            using var services = new ServiceCollection().BuildServiceProvider();

            var create = () => AtdlPanel.Create(TestStrategies.MinimalOneControlStrategy(), services);

            create.Should().Throw<InvalidOperationException>().WithMessage("*StrategyPanelRenderer*");
        });
    }

    [Theory]
    [InlineData(63, true)]
    [InlineData(64, false)]
    public void Render_PanelNestingBoundary_IsExactlyMaxDepth(int children, bool accepted)
    {
        // Observed: the root counts toward the 64-panel limit, so 63 nested children render and 64 are refused.
        // RenderFailureContractTests.Render_RejectsExcessivelyNestedPanels (65) is not a tight boundary.
        StaTestHarness.Run(() =>
        {
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var strategy = TestStrategies.MinimalOneControlStrategy();
            var panel = strategy.StrategyLayout.StrategyPanel;
            panel.Controls.Clear();
            for (var i = 0; i < children; i++)
            {
                var child = new FixPortal.FixAtdl.Model.Elements.StrategyPanel_t(strategy);
                panel.StrategyPanels.Add(child);
                panel = child;
            }

            var renderer = services.GetRequiredService<Rendering.StrategyPanelRenderer>();
            var render = () => renderer.Render(strategy, services);

            if (accepted)
            {
                render.Should().NotThrow();
            }
            else
            {
                render.Should().Throw<RenderingException>().WithMessage("*nesting*");
            }
        });
    }

    [Theory]
    [InlineData("{x:Static sys:Environment.MachineName}")]
    [InlineData("{Binding Path=Nope}")]
    [InlineData("\"/><Button Content=\"x")]
    public void Create_RendersVenueToolTipAsLiteralText(string toolTip)
    {
        StaTestHarness.Run(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            strategy.StrategyLayout.StrategyPanel.Controls[0].ToolTip = toolTip;
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();

            var (view, _) = AtdlPanel.Create(strategy, services);
            view.Measure(new System.Windows.Size(800, 600));
            view.Arrange(new System.Windows.Rect(0, 0, 800, 600));
            view.UpdateLayout();

            var element = (System.Windows.FrameworkElement)view.FindName(Rendering.WpfControlRenderer.CleanName("Qty"));
            element.ToolTip.Should().Be(toolTip);
        });
    }
}
