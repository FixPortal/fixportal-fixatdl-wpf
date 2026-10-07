using FixPortal.FixAtdl.Wpf.Rendering;
using FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf;

/// <summary>
/// Composition-root entry point for this library. The host application's composition
/// root calls <see cref="AddFixAtdlWpf"/> once to register the strategy-panel renderer and every default
/// control renderer; the exact method name/signature is part of the public surface hosts depend on.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFixAtdlWpf(this IServiceCollection services)
    {
        services.AddTransient<StrategyPanelRenderer>();

        services.AddTransient<IControlRenderer, CheckBoxRenderer>();
        services.AddTransient<IControlRenderer, CheckBoxListRenderer>();
        services.AddTransient<IControlRenderer, ClockRenderer>();
        services.AddTransient<IControlRenderer, DoubleSpinnerRenderer>();
        services.AddTransient<IControlRenderer, DropDownListRenderer>();
        services.AddTransient<IControlRenderer, EditableDropDownListRenderer>();
        services.AddTransient<IControlRenderer, HiddenFieldRenderer>();
        services.AddTransient<IControlRenderer, LabelRenderer>();
        services.AddTransient<IControlRenderer, MultiSelectListRenderer>();
        services.AddTransient<IControlRenderer, RadioButtonRenderer>();
        services.AddTransient<IControlRenderer, RadioButtonListRenderer>();
        services.AddTransient<IControlRenderer, SingleSelectListRenderer>();
        services.AddTransient<IControlRenderer, SingleSpinnerRenderer>();
        services.AddTransient<IControlRenderer, SliderRenderer>();
        services.AddTransient<IControlRenderer, TextFieldRenderer>();

        return services;
    }
}
