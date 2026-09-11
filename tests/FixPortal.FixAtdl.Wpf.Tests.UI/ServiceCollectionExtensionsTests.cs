using AwesomeAssertions;
using FixPortal.FixAtdl.Wpf.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// See ResourceDictionaryLoadTests for why WPF-touching tests run on an explicit STA thread
/// rather than the default MTA test-runner thread.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddFixAtdlWpf_RegistersStrategyPanelRenderer()
    {
        RunOnSta(() =>
        {
            var services = new ServiceCollection();

            services.AddFixAtdlWpf();
            var provider = services.BuildServiceProvider();

            provider.GetService<StrategyPanelRenderer>().Should().NotBeNull();
        });
    }

    [Fact]
    public void AddFixAtdlWpf_RegistersAllDefaultControlRenderers()
    {
        RunOnSta(() =>
        {
            var services = new ServiceCollection();

            services.AddFixAtdlWpf();
            var provider = services.BuildServiceProvider();

            provider.GetServices<IControlRenderer>().Should().NotBeEmpty();
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
