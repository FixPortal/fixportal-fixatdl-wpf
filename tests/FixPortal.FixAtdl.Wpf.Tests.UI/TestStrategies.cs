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

    /// <summary>
    /// A strategy carrying a <see cref="DoubleSpinner_t"/> with distinct inner/outer increments and a
    /// <see cref="Label_t"/> with text - the two control types whose emitted markup no render test
    /// parsed (audit F11), wired through the real StrategyPanel_t.
    /// </summary>
    public static Strategy_t DoubleSpinnerAndLabelStrategy()
    {
        var strategy = new Strategy_t();
        var panel = new StrategyPanel_t(strategy);

        strategy.StrategyLayout = new StrategyLayout_t { StrategyPanel = panel };

        var spinner = new DoubleSpinner_t("Step")
        {
            ParameterRef = "Step",
            InnerIncrement = 5m,
            OuterIncrement = 0.25m,
        };
        var label = new Label_t("Info") { Label = "Info text" };
        label.LoadInitValue(FixPortal.FixAtdl.Fix.FixFieldValueProvider.Empty);

        panel.Controls.Add(spinner);
        panel.Controls.Add(label);
        strategy.Parameters.Add(new Parameter_t<Float_t>("Step") { FixTag = 9004 });

        return strategy;
    }
}
