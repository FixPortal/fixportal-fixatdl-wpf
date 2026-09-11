using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Model.Types;

namespace FixPortal.FixAtdl.Wpf.Core.Tests;

/// <summary>
/// Builds minimal, wired-up <see cref="Control_t"/>/<see cref="Strategy_t"/> fixtures using the core
/// package's real construction API (StrategyPanel_t.Controls / ParameterCollection), rather than mocks,
/// so ViewModel tests exercise the actual ATDL constraint-validation path
/// (<see cref="Model.Elements.Support.IParameter.SetValueFromControl"/>).
/// </summary>
internal static class TestControls
{
    /// <summary>
    /// A standalone required numeric control (min=0), not attached to any strategy. Suitable for
    /// <see cref="ViewModels.ControlViewModel"/> tests that only need the control + its referenced parameter.
    /// </summary>
    public static (SingleSpinner_t Control, Parameter_t<Int_t> Parameter) RequiredNumericControl()
    {
        var control = new SingleSpinner_t("Qty") { ParameterRef = "Qty" };
        var parameter = new Parameter_t<Int_t>("Qty") { Use = Use_t.Required };

        parameter.Value.MinValue = 0;

        return (control, parameter);
    }

    /// <summary>
    /// A minimal strategy with a single required numeric control, wired through the real
    /// StrategyPanel_t/ParameterCollection plumbing so <c>strategy.Controls</c> is populated.
    /// </summary>
    public static Strategy_t MinimalStrategyWithOneRequiredControl()
    {
        var strategy = new Strategy_t();
        var panel = new StrategyPanel_t(strategy);

        strategy.StrategyLayout = new StrategyLayout_t { StrategyPanel = panel };

        var (control, parameter) = RequiredNumericControl();

        panel.Controls.Add(control);
        strategy.Parameters.Add(parameter);

        return strategy;
    }
}
