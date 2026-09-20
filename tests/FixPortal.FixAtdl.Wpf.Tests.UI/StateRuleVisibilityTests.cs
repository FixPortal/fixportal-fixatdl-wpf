using System.Windows;
using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Enumerations;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// Audit F13: ControlViewModel.Visibility supplies the 'Visible'/'Collapsed' literal that rendered
/// XAML binds to the element's Visibility dependency property. That contract was never asserted at
/// the WPF level - a mistyped literal fails as a silent binding error and the element stays visible.
/// This test renders a control with a hiding state rule, fires the rule through the real
/// fixpoint pass, and asserts the rendered element actually collapses.
/// </summary>
public class StateRuleVisibilityTests
{
    [Fact]
    public void HidingStateRule_CollapsesRenderedElement()
    {
        StaTestHarness.Run(() =>
        {
            var strategy = new Strategy_t();
            var panel = new StrategyPanel_t(strategy);
            strategy.StrategyLayout = new StrategyLayout_t { StrategyPanel = panel };

            var toggle = new CheckBox_t("Toggle");
            var hidden = new SingleSpinner_t("Hidden");
            hidden.StateRules.Add(
                new StateRule_t
                {
                    Visible = false,
                    Edit = new Edit_t<Control_t>
                    {
                        Field = "Toggle",
                        Operator = Operator_t.Equal,
                        Value = "true",
                    },
                }
            );
            panel.Controls.Add(toggle);
            panel.Controls.Add(hidden);

            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(strategy, services);
            Layout(view);
            var element = (FrameworkElement)view.FindName(Rendering.WpfControlRenderer.CleanName("Hidden"));

            element.Visibility.Should().Be(Visibility.Visible, "the rule is inactive before the toggle fires");

            model.Controls[0].Value = true;

            element.Visibility.Should().Be(Visibility.Collapsed);
        });
    }

    private static void Layout(FrameworkElement view)
    {
        view.Measure(new Size(800, 600));
        view.Arrange(new Rect(0, 0, 800, 600));
        view.UpdateLayout();
    }
}
