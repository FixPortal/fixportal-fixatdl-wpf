using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AwesomeAssertions;
using FixPortal.FixAtdl.Fix;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Model.Types;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// A required parameter must be signalled by something other than colour. Upstream Atdl4net drove a
/// required marker off the label; the port dropped it and left a MistyRose background as the only
/// cue, which conveys nothing to a colour-blind trader, on a washed-out screen, or under a
/// high-contrast theme. On an order-entry panel that is a defect, not a nicety - so the cue is
/// pinned by a test rather than left to survive the next restyle on luck.
/// </summary>
public class RequiredFieldCueTests
{
    private static Strategy_t StrategyWithQuantity(Use_t use, string label = "Quantity")
    {
        var strategy = new Strategy_t();
        var panel = new StrategyPanel_t(strategy);
        strategy.StrategyLayout = new StrategyLayout_t { StrategyPanel = panel };

        var control = new SingleSpinner_t("Qty") { ParameterRef = "Qty", Label = label };
        var parameter = new Parameter_t<Float_t>("Qty") { FixTag = new FixTag(38), Use = use };

        panel.Controls.Add(control);
        strategy.Parameters.Add(parameter);

        return strategy;
    }

    private static Label QuantityLabel(DependencyObject root) =>
        Descendants(root).OfType<Label>().Single(label => Equals(label.Content, "Quantity"));

    [Fact]
    public void A_required_parameters_label_carries_a_non_colour_cue()
    {
        StaTestHarness.Run(() =>
        {
            var strategy = StrategyWithQuantity(Use_t.Required);
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, _) = AtdlPanel.Create(strategy, services);
            Layout(view);

            var label = QuantityLabel(view);

            // The venue's own label text is untouched - the marker is applied by format, so no
            // caller ever sees "Quantity *" where it expected "Quantity".
            label.Content.Should().Be("Quantity");
            label.ContentStringFormat.Should().Be("{0} *");
        });
    }

    [Fact]
    public void An_optional_parameters_label_carries_no_cue()
    {
        StaTestHarness.Run(() =>
        {
            var strategy = StrategyWithQuantity(Use_t.Optional);
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, _) = AtdlPanel.Create(strategy, services);
            Layout(view);

            var label = QuantityLabel(view);

            label.Content.Should().Be("Quantity");
            label.ContentStringFormat.Should().BeNull();
        });
    }

    [Theory]
    [InlineData("Quantity*")]
    [InlineData("Quantity *")]
    [InlineData("Urgency:*")]
    public void A_label_that_already_marks_itself_required_is_not_marked_twice(string venueLabel)
    {
        // Real venue documents do this - the FIXatdl conformance corpus carries label="Urgency:*" on a
        // use="required" parameter. Adding our own marker on top renders "Urgency:* *".
        StaTestHarness.Run(() =>
        {
            var strategy = StrategyWithQuantity(Use_t.Required, venueLabel);
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, _) = AtdlPanel.Create(strategy, services);
            Layout(view);

            var label = Descendants(view).OfType<Label>().Single(l => Equals(l.Content, venueLabel));

            label.ContentStringFormat.Should().BeNull();
        });
    }

    [Fact]
    public void A_required_control_is_not_painted_a_different_colour_from_an_optional_one()
    {
        // The library used to tint required fields MistyRose. A pinned light-theme colour cannot
        // survive a themed host: under WPF's Fluent dark theme that tint was carried into the
        // ComboBox dropdown while Fluent supplied a near-white foreground, leaving the list
        // unreadable. The asterisk carries the requirement; nothing here paints it.
        StaTestHarness.Run(() =>
        {
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();

            var (requiredView, _) = AtdlPanel.Create(StrategyWithQuantity(Use_t.Required), services);
            var (optionalView, _) = AtdlPanel.Create(StrategyWithQuantity(Use_t.Optional), services);
            Layout(requiredView);
            Layout(optionalView);

            var required = Descendants(requiredView).OfType<Control>().Select(c => c.Background?.ToString()).ToList();
            var optional = Descendants(optionalView).OfType<Control>().Select(c => c.Background?.ToString()).ToList();

            required.Should().BeEquivalentTo(optional);
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
