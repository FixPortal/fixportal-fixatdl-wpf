using AwesomeAssertions;
using FixPortal.FixAtdl.Diagnostics.Exceptions;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

public class RealUpstreamFailureTests
{
    [Fact]
    public void RealNumericControlInternalFailure_PropagatesAndBlocksReadBack()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        var parameter = strategy.Parameters["Qty"];
        parameter.FixTag = 9001;
        var model = new EditViewModel(strategy);
        var control = model.Controls.Single();
        control.Value = 12m;

        model.HasErrors.Should().BeFalse();
        control.HasErrors.Should().BeFalse();
        control.Value.Should().Be(12m);
        control.UnderlyingControl.GetCurrentValue().Should().Be(12m);
        parameter.WireValue.Should().Be("12");
        model.ReadBackFixValues().Should().ContainSingle().Which.Should().Be(new KeyValuePair<int, string>(9001, "12"));

        var edit = () => control.Value = true;

        edit.Should().Throw<InternalErrorException>().WithMessage("*Unexpected type System.Boolean*");
        // The public VM value was assigned before validation; no rollback is promised.
        control.Value.Should().Be(true);
        control.UnderlyingControl.GetCurrentValue().Should().Be(12m);
        parameter.WireValue.Should().Be("12");
        model.HasErrors.Should().BeTrue();
        control.HasErrors.Should().BeTrue();
        control
            .GetErrors(nameof(control.Value))
            .Should()
            .ContainSingle()
            .Which.ErrorMessage.Should()
            .Be("The value could not be applied; reload the strategy before submitting.");
        var direct = () => model.ReadBackFixValues();
        var group = () => model.ReadBackStrategyParametersGrp();
        direct.Should().Throw<InvalidOperationException>().WithMessage("*validation errors*FIX values*");
        group.Should().Throw<InvalidOperationException>().WithMessage("*validation errors*FIX values*");
    }
}
