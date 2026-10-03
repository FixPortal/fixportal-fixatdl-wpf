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
        // Render BEFORE building the view model, because rendering is the step that can fail and the
        // view model's constructor is the step that mutates the caller's strategy - it loads init
        // values and applies state rules through the controls. In the other order a render failure
        // left the strategy half-initialized, and a caller that caught and retried on the same
        // instance got fresh RuleState objects that snapshot the already-rule-applied value as their
        // restore point, so a later {NULL} deactivation put back the rule's value rather than the
        // document's. Rendering reads only structure - control types, ids, increments, and which
        // parameters are required - and every Visibility and IsEnabled it writes is a binding
        // expression resolved later against the view model, so nothing here depends on the values
        // the view model would have established.
        var renderer = services.GetRequiredService<StrategyPanelRenderer>();
        var view =
            renderer.Render(strategy, services)
            ?? throw ThrowHelper.New<RenderingException>(
                ExceptionContext,
                "Strategy panel rendering produced no view."
            );

        var viewModel = new EditViewModel(strategy, isAmendment);

        view.DataContext = viewModel;

        return (view, viewModel);
    }
}
