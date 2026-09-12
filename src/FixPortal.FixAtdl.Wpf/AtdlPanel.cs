using System.Windows;
using FixPortal.FixAtdl.Diagnostics.Exceptions;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;
using FixPortal.FixAtdl.Wpf.Rendering;
using Microsoft.Extensions.DependencyInjection;
using ThrowHelper = FixPortal.FixAtdl.Diagnostics.ThrowHelper;

namespace FixPortal.FixAtdl.Wpf;

/// <summary>
/// The library's public entry point: builds the renderable panel and its <see cref="EditViewModel"/> for a
/// given <see cref="Strategy_t"/>. The host calls <see cref="Create"/> to get both, wires the returned
/// <see cref="EditViewModel"/> into its own UI as needed, and reads submit-time values and
/// <see cref="EditViewModel.HasErrors"/> back off it.
/// </summary>
public static class AtdlPanel
{
    private const string ExceptionContext = nameof(AtdlPanel);

    /// <summary>Creates a panel on the WPF dispatcher thread, preserving supplied control values.</summary>
    /// <remarks>Initialize a new strategy with Strategy_t.LoadInitialControlValues before calling.
    /// Use a separate mutable strategy instance for each editor.</remarks>
    public static (FrameworkElement View, EditViewModel ViewModel) Create(
        Strategy_t strategy,
        IServiceProvider services
    ) => Create(strategy, services, false);

    /// <summary>Creates an editor, preserving immutable parameter values when amending an order.</summary>
    public static (FrameworkElement View, EditViewModel ViewModel) Create(
        Strategy_t strategy,
        IServiceProvider services,
        bool isAmendment
    )
    {
        var viewModel = new EditViewModel(strategy, isAmendment);
        var renderer = services.GetRequiredService<StrategyPanelRenderer>();
        var view =
            renderer.Render(strategy, services)
            ?? throw ThrowHelper.New<RenderingException>(
                ExceptionContext,
                "Strategy panel rendering produced no view."
            );

        view.DataContext = viewModel;

        return (view, viewModel);
    }
}
