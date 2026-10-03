using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Model.Types;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// Audit A26: for horizontally-oriented panels StrategyPanelRenderer emits one more Grid column than
/// there are controls and gives the last column star width, then emits a dummy Rectangle into that
/// column. The star column absorbs leftover width so the content columns size to their controls; the
/// dummy element keeps the star column from collapsing when the content is narrower. Only implicitly
/// exercised before - no test rendered a horizontal panel and asserted the star column. This test
/// pins both halves of the emission on the produced Grid.
/// </summary>
public class HorizontalPanelSizingTests
{
    [Fact]
    public void HorizontalPanel_StarColumnWithDummyRectangle()
    {
        StaTestHarness.Run(() =>
        {
            var strategy = new Strategy_t();
            var panel = new StrategyPanel_t(strategy) { Orientation = Orientation_t.Horizontal };
            strategy.StrategyLayout = new StrategyLayout_t { StrategyPanel = panel };
            panel.Controls.Add(new SingleSpinner_t("Qty") { ParameterRef = "Qty" });
            strategy.Parameters.Add(new Parameter_t<Float_t>("Qty") { FixTag = 9001 });

            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, _) = AtdlPanel.Create(strategy, services);
            Layout(view);

            var grid = (Grid)((Controls.StrategyPanelFrame)view).Content;

            // One control -> two columns: the content column sizes to the control, the trailing
            // star column absorbs the remaining width.
            grid.ColumnDefinitions.Should().HaveCount(2);
            grid.ColumnDefinitions[0].Width.IsAuto.Should().BeTrue();
            grid.ColumnDefinitions[1].Width.IsStar.Should().BeTrue();

            // The dummy Rectangle occupies the star column so it cannot collapse to nothing.
            var rectangle = grid.Children.OfType<Rectangle>().Single();
            Grid.GetColumn(rectangle).Should().Be(1);
        });
    }

    private static void Layout(FrameworkElement view)
    {
        view.Measure(new Size(800, 600));
        view.Arrange(new Rect(0, 0, 800, 600));
        view.UpdateLayout();
    }
}
