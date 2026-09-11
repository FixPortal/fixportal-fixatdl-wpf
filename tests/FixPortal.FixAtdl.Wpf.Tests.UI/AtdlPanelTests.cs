using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// See ResourceDictionaryLoadTests for why WPF-touching tests run on an explicit STA thread rather
/// than the default MTA test-runner thread.
/// </summary>
public class AtdlPanelTests
{
    [Fact]
    public void Create_ReturnsViewAndViewModelForStrategy()
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();

            var (view, viewModel) = AtdlPanel.Create(strategy, services);

            view.Should().NotBeNull();
            viewModel.Controls.Should().HaveCount(1);
        });
    }

    [Fact]
    public void ReadBackFixValues_ReturnsEditedValueKeyedByTag()
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (_, viewModel) = AtdlPanel.Create(strategy, services);

            viewModel.Controls[0].Value = 12.5m;
            var values = viewModel.ReadBackFixValues();

            values.Should().ContainKey(TestStrategies.QtyFixTag).WhoseValue.Should().Be("12.5");
        });
    }

    private static void RunOnSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure != null)
        {
            throw failure;
        }
    }
}
