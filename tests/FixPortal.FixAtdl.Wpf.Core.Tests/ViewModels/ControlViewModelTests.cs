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

    [Fact]
    public void SettingBoolValue_OnCheckBoxControl_DoesNotThrow()
    {
        // Regression test: ConvertForControl previously force-converted every non-null/decimal/string
        // value (including bool) via Convert.ToDecimal before handing it to Control_t.SetValue, which
        // BinaryControlBase (CheckBox_t's base) rejects — throwing instead of failing validation cleanly.
        var control = new FixPortal.FixAtdl.Model.Controls.CheckBox_t("Confirm");
        var act = () => _ = new ControlViewModel(control) { Value = true };

        act.Should().NotThrow();
    }
}
