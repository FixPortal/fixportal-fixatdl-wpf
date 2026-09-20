using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Wpf.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddFixAtdlWpf_RegistersStrategyPanelRenderer()
    {
        StaTestHarness.Run(() =>
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
        StaTestHarness.Run(() =>
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
}
