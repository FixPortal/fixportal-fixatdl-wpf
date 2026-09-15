using System.Windows;
using System.Windows.Controls;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// Audit A42 (live half): LabelRenderer emits Content="{Binding Path=Controls[i].Value}". U3 pinned
/// the initial render; this test pins that the binding is live - driving a new value through the
/// view model updates the rendered label - and thereby also that a mistyped binding path renders
/// blank rather than throwing, since both directions flow through the same emitted attribute.
/// </summary>
public class LabelLiveBindingTests
{
    [Fact]
    public void LabelContent_UpdatesWhenModelValueChanges()
    {
        StaTestHarness.Run(() =>
        {
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(TestStrategies.DoubleSpinnerAndLabelStrategy(), services);
            Layout(view);
            var label = Descendants(view).OfType<Label>().Single();

            label.Content.Should().Be("Info text");

            model.Controls[1].Value = "2026-09-15";

            label.Content.Should().Be("2026-09-15");
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
