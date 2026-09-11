using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Types;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// Builds minimal, wired-up <see cref="Strategy_t"/> fixtures using the core package's real
/// construction API, mirroring FixPortal.FixAtdl.Wpf.Core.Tests.TestControls (which is internal to
/// that assembly and not visible here).
/// </summary>
internal static class TestStrategies
{
    public const int QtyFixTag = 6218;

    /// <summary>
    /// A minimal strategy with a single numeric control (a <see cref="SingleSpinner_t"/>) referencing a
    /// parameter with FIX tag <see cref="QtyFixTag"/>, wired through the real StrategyPanel_t so both
    /// rendering and view-model construction can exercise it.
    /// </summary>
    public static Strategy_t MinimalOneControlStrategy()
    {
        var strategy = new Strategy_t();
        var panel = new StrategyPanel_t(strategy);

        strategy.StrategyLayout = new StrategyLayout_t { StrategyPanel = panel };

        var control = new SingleSpinner_t("Qty") { ParameterRef = "Qty" };
        var parameter = new Parameter_t<Float_t>("Qty") { FixTag = QtyFixTag };

        panel.Controls.Add(control);
        strategy.Parameters.Add(parameter);

        return strategy;
    }
}
