using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// Audit F11: DoubleSpinner_t and Label_t appeared only in the DI-registration equivalence list -
/// no render test ever parsed their markup (DoubleSpinnerRenderer's InnerIncrement/OuterIncrement
/// emit, LabelRenderer's Content/AutomationId emit), so a mistyped attribute or renamed dependency
/// property surfaced only at a host. These tests render both through the real AtdlPanel pipeline
/// and pin the emitted attribute-to-dependency-property contracts on the live elements: the
/// DoubleSpinner's InnerIncrement/OuterIncrement attributes resolve against its DPs via the
/// UnderlyingControl bindings, and the Label's Content and AutomationProperties.AutomationId
/// attributes resolve against the control id and the view model's Value.
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
