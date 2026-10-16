using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Threading;
using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Model.Types;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

public partial class AtdlPanelTests
{
    [Theory]
    [InlineData("Order Qty", "{Binding Path=Tag}")]
    [InlineData("Order.Qty-1", "{}literal & <text>")]
    [InlineData("X, Source={x:Static sys:Environment.MachineName}", "{x:Static sys:Environment.MachineName}")]
    public void Create_HostileIdsAndRequiredLabelsRemainLiteralAndResolveTheirActualTargets(string id, string text)
    {
        StaTestHarness.Run(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            var panel = strategy.StrategyLayout.StrategyPanel;
            panel.Controls.Clear();
            panel.Controls.Add(new SingleSpinner_t(id) { ParameterRef = "Qty", Label = text });
            var checkboxId = id + "!";
            panel.Controls.Add(new CheckBox_t(checkboxId) { ParameterRef = "Flag", Label = text });
            strategy.Parameters[0].Use = Use_t.Required;
            strategy.Parameters.Add(new Parameter_t<Boolean_t>("Flag") { FixTag = 6219, Use = Use_t.Required });
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();

            var (view, model) = AtdlPanel.Create(strategy, services);
            view.Measure(new Size(800, 600));
            view.Arrange(new Rect(0, 0, 800, 600));
            view.UpdateLayout();
            view.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

            var elements = Descendants(view).OfType<FrameworkElement>().ToList();
            var spinner = elements.OfType<Controls.SingleSpinner>().Single();
            var checkbox = elements.OfType<CheckBox>().Single();
            // UTF-16 byte encoding is independent of the renderer's CleanName helper.
            var spinnerName = "Control_" + Convert.ToHexString(Encoding.BigEndianUnicode.GetBytes(id));
            var checkboxName = "Control_" + Convert.ToHexString(Encoding.BigEndianUnicode.GetBytes(checkboxId));
            spinner.Name.Should().Be(spinnerName);
            checkbox.Name.Should().Be(checkboxName).And.NotBe(spinner.Name);
            spinner.Name.Should().MatchRegex("^Control_[0-9A-F]+$");
            checkbox.Name.Should().MatchRegex("^Control_[0-9A-F]+$");
            view.FindName(spinnerName).Should().BeSameAs(spinner);
            view.FindName(checkboxName).Should().BeSameAs(checkbox);
            spinner.DataContext.Should().BeSameAs(model.Controls[0]);
            checkbox.DataContext.Should().BeSameAs(model.Controls[1]);
            checkbox.Content.Should().Be(text);
            AutomationProperties.GetName(checkbox).Should().Be(text + " *");
            var label = elements.OfType<Label>().Single(candidate => Equals(candidate.Content, text));
            label.Content.Should().Be(text);
            label
                .Target.Should()
                .BeSameAs(spinner, "the label must resolve its encoded ElementName binding after layout");
            AutomationProperties.GetName(label).Should().Be(text + " *");
        });
    }
}
