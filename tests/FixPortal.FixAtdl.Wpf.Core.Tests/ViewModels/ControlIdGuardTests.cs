using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

/// <summary>Audit 2026-10-11 A12: the EditViewModel control-id guard had no direct test.</summary>
public class ControlIdGuardTests
{
    [Fact]
    public void EmptyControlId_IsRejectedAtConstruction()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.StrategyLayout.StrategyPanel.Controls.Add(new CheckBox_t(""));

        var create = () => new EditViewModel(strategy);

        create.Should().Throw<ArgumentException>().WithMessage("*nonempty and unique*");
    }
}
