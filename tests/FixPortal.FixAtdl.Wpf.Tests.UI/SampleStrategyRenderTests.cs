using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Xml;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// The document the sample application ships is rendered here, from the same file, so it cannot rot
/// unnoticed. A sample that no longer parses is worse than no sample: it is the first thing anyone
/// evaluating the package runs, and this suite is the only thing standing between a bad edit and
/// that first impression.
/// </summary>
public class SampleStrategyRenderTests
{
    private static Strategy_t LoadSampleStrategy()
    {
        using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Strategies", "sample-strategy.xml"));
        return new StrategiesReader().Load(stream).Strategies[0];
    }

    [Fact]
    public void The_shipped_sample_document_parses()
    {
        var strategy = LoadSampleStrategy();

        strategy.Name.Should().Be("Participate");
        strategy.Controls.Should().HaveCountGreaterThan(10, "the sample exists to show a wide control spread");
    }

    [Fact]
    public void The_shipped_sample_document_renders_every_control()
    {
        StaTestHarness.Run(() =>
        {
            var strategy = LoadSampleStrategy();
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();

            var (view, model) = AtdlPanel.Create(strategy, services);
            view.Measure(new Size(900, 700));
            view.Arrange(new Rect(0, 0, 900, 700));
            view.UpdateLayout();

            model.Controls.Should().HaveCount(strategy.Controls.Count());
            var renderedElements = Descendants(view).OfType<FrameworkElement>().ToArray();
            renderedElements.Should().NotBeEmpty();

            foreach (var control in model.Controls)
            {
                var expectedName = Rendering.WpfControlRenderer.CleanName(control.Id);
                var element = renderedElements.SingleOrDefault(candidate =>
                    candidate.Name == expectedName || AutomationProperties.GetAutomationId(candidate) == expectedName
                );

                element.Should().NotBeNull($"control {control.Id} must have a rendered element");
                element.DataContext.Should().BeSameAs(control);
            }
        });
    }

    [Fact]
    public void The_shipped_sample_marks_its_required_fields_without_relying_on_colour()
    {
        StaTestHarness.Run(() =>
        {
            var strategy = LoadSampleStrategy();
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();

            var (view, _) = AtdlPanel.Create(strategy, services);
            view.Measure(new Size(900, 700));
            view.Arrange(new Rect(0, 0, 900, 700));
            view.UpdateLayout();

            // ParticipationRate and Urgency are use="required" in the sample document.
            var marked = Descendants(view)
                .OfType<Label>()
                .Where(label => label.ContentStringFormat == "{0} *")
                .Select(label => label.Content?.ToString())
                .ToList();

            marked.Should().BeEquivalentTo("Rate (% of volume, 1-50)", "Urgency");
        });
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            yield return child;
            foreach (var descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }
}
