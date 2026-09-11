using AwesomeAssertions;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

public class EditViewModelTests
{
    [Fact]
    public void HasErrors_TrueWhenAnyChildControlInvalid()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        var viewModel = new EditViewModel(strategy);

        viewModel.Controls[0].Value = null; // required control left empty

        viewModel.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void HasErrors_FalseWhenAllChildControlsValid()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        var viewModel = new EditViewModel(strategy);

        viewModel.Controls[0].Value = 5;

        viewModel.HasErrors.Should().BeFalse();
    }
}
