using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Wpf.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// WPF controls require an STA thread; the test runner uses MTA threads.
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

            provider
                .GetServices<IControlRenderer>()
                .Select(renderer => renderer.ControlType)
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        typeof(CheckBox_t),
                        typeof(CheckBoxList_t),
                        typeof(Clock_t),
                        typeof(DoubleSpinner_t),
                        typeof(DropDownList_t),
                        typeof(EditableDropDownList_t),
                        typeof(HiddenField_t),
                        typeof(Label_t),
                        typeof(MultiSelectList_t),
                        typeof(RadioButton_t),
                        typeof(RadioButtonList_t),
                        typeof(SingleSelectList_t),
                        typeof(SingleSpinner_t),
                        typeof(Slider_t),
                        typeof(TextField_t),
                    }
                )
                .And.OnlyHaveUniqueItems();
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
        thread.IsBackground = true;
        thread.Start();
        thread.Join(TimeSpan.FromSeconds(30)).Should().BeTrue("WPF checks must finish within 30 seconds");

        if (failure != null)
        {
            throw failure;
        }
    }
}
