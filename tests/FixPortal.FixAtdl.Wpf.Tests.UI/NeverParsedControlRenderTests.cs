using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// Verifies that DoubleSpinner and Label markup resolves their emitted attributes against the
/// live controls' dependency properties through the real AtdlPanel pipeline.
/// </summary>
public class NeverParsedControlRenderTests
{
    [Fact]
    public void DoubleSpinnerMarkup_ResolvesIncrementBindings()
    {
        StaTestHarness.Run(() =>
        {
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, _) = AtdlPanel.Create(TestStrategies.DoubleSpinnerAndLabelStrategy(), services);
            Layout(view);

            var spinner = (Controls.DoubleSpinner)view.FindName(Rendering.WpfControlRenderer.CleanName("Step"));

            spinner.InnerIncrement.Should().Be(5m);
            spinner.OuterIncrement.Should().Be(0.25m);
        });
    }

    [Fact]
    public void LabelMarkup_ResolvesContentAndAutomationId()
    {
        StaTestHarness.Run(() =>
        {
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, _) = AtdlPanel.Create(TestStrategies.DoubleSpinnerAndLabelStrategy(), services);
            Layout(view);

            var label = Descendants(view).OfType<Label>().Single();

            AutomationProperties.GetAutomationId(label).Should().Be(Rendering.WpfControlRenderer.CleanName("Info"));
            label.Content.Should().Be("Info text");
        });
    }

    private static void Layout(FrameworkElement view)
    {
        view.Measure(new Size(800, 600));
        view.Arrange(new Rect(0, 0, 800, 600));
        view.UpdateLayout();
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
        {
            yield return child;

            foreach (var descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }
}
