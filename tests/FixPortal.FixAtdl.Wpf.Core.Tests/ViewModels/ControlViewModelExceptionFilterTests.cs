using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Elements.Support;
using FixPortal.FixAtdl.Validation;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;
using NSubstitute;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

public class ControlViewModelExceptionFilterTests
{
    [Fact]
    public void ExceptionNotInFilter_FromParameter_DuringValueConversion_Propagates()
    {
        var control = new TextField_t("Note");
        var parameter = Substitute.For<IParameter>();
        // Construction performs an initial validation pass against the null current value; only the
        // conversion triggered by assigning a value must throw.
        var calls = 0;
        parameter
            .SetValueFromControl(Arg.Any<Control_t>())
            .Returns(_ =>
                ++calls == 1
                    ? ValidationResult.ValidResult
                    : throw new InvalidOperationException("parameter store unavailable")
            );
        var model = new ControlViewModel(control, parameter);

        var act = () => model.Value = "anything";

        act.Should().Throw<InvalidOperationException>().WithMessage("parameter store unavailable");
    }

    [Fact]
    public void ExceptionInFilter_FromParameter_BecomesValidationError()
    {
        var control = new TextField_t("Note");
        var parameter = Substitute.For<IParameter>();
        parameter.SetValueFromControl(Arg.Any<Control_t>()).Returns(_ => throw new FormatException("bad wire format"));
        var model = new ControlViewModel(control, parameter);

        var act = () => model.Value = "anything";

        act.Should().NotThrow();
        model.HasErrors.Should().BeTrue();
    }
}
