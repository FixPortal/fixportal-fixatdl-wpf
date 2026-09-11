using AwesomeAssertions;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

public class ControlViewModelTests
{
    [Fact]
    public void SettingValueOutsideConstraint_SetsHasErrorsTrue()
    {
        var (control, parameter) = TestControls.RequiredNumericControl();
        var viewModel = new ControlViewModel(control, parameter) { Value = -1 };

        viewModel.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void SettingValidValue_ClearsErrors()
    {
        var (control, parameter) = TestControls.RequiredNumericControl();
        var viewModel = new ControlViewModel(control, parameter) { Value = -1 };

        viewModel.Value = 5;

        viewModel.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void SettingNullValue_OnRequiredControl_SetsHasErrorsTrue()
    {
        var (control, parameter) = TestControls.RequiredNumericControl();
        var viewModel = new ControlViewModel(control, parameter) { Value = 5 };

        viewModel.Value = null;

        viewModel.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void ControlWithNoReferencedParameter_NeverErrors()
    {
        var control = new FixPortal.FixAtdl.Model.Controls.TextField_t("Note");
        var viewModel = new ControlViewModel(control) { Value = "anything" };

        viewModel.HasErrors.Should().BeFalse();
    }
}
