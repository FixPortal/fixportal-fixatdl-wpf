using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Types;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

/// <summary>
/// FIXatdl 1.1 StrategyParametersGrp (tags 957-960) read-back via <see cref="EditViewModel"/>,
/// for hosts using Tag957Support transport rather than (or in addition to) direct FIX tags.
/// </summary>
public class StrategyParametersGrpTests
{
    private static Strategy_t MinimalStrategyWithOneOptionalControl()
    {
        var strategy = new Strategy_t();
        var panel = new StrategyPanel_t(strategy);
        strategy.StrategyLayout = new StrategyLayout_t { StrategyPanel = panel };

        var control = new SingleSpinner_t("Qty") { ParameterRef = "Qty" };
        var parameter = new Parameter_t<Int_t>("Qty");

        panel.Controls.Add(control);
        strategy.Parameters.Add(parameter);

        return strategy;
    }

    [Fact]
    public void ReadBackStrategyParametersGrp_OmitsTag957WhenNoParameterIsSet()
    {
        var strategy = MinimalStrategyWithOneOptionalControl();
        var model = new EditViewModel(strategy);

        model.ReadBackStrategyParametersGrp().Should().BeEmpty();
    }

    [Fact]
    public void ReadBackStrategyParametersGrp_EmitsTheFilledParameter()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        var model = new EditViewModel(strategy);

        model.Controls[0].Value = 12m;

        model.ReadBackStrategyParametersGrp().Should().Equal((957, "1"), (958, "Qty"), (959, "1"), (960, "12"));
    }

    [Fact]
    public void ReadBackStrategyParametersGrp_ThrowsWhenStrategyHasErrors()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        var model = new EditViewModel(strategy);

        model.HasErrors.Should().BeTrue("the required Qty control is unset");
        var read = () => model.ReadBackStrategyParametersGrp();

        read.Should().Throw<InvalidOperationException>();
    }
}
