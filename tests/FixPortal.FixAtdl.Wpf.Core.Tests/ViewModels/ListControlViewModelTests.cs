using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Model.Types;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

public class ListControlViewModelTests
{
    private static (SingleSelectList_t Control, Parameter_t<Char_t> Parameter) RequiredSideControl()
    {
        var control = new SingleSelectList_t("Side") { ParameterRef = "Side" };

        control.ListItems.Add(new ListItem_t { EnumId = "1", UiRep = "Buy" });
        control.ListItems.Add(new ListItem_t { EnumId = "2", UiRep = "Sell" });

        var parameter = new Parameter_t<Char_t>("Side") { Use = Use_t.Required };

        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "1", WireValue = "1" });
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "2", WireValue = "2" });

        return (control, parameter);
    }

    [Fact]
    public void Items_ReflectsControlListItems()
    {
        var (control, parameter) = RequiredSideControl();
        var viewModel = new ListControlViewModel(control, parameter);

        viewModel.Items.Select(i => i.EnumId).Should().BeEquivalentTo("1", "2");
    }

    [Fact]
    public void SelectingAnItem_SelectsOnlyThatItem()
    {
        var (control, parameter) = RequiredSideControl();
        var viewModel = new ListControlViewModel(control, parameter);

        viewModel.Items[0].IsSelected = true;

        viewModel.Items[0].IsSelected.Should().BeTrue();
        viewModel.Items[1].IsSelected.Should().BeFalse();
        viewModel.SelectedValue.Should().Be("1");
    }

    [Fact]
    public void SelectingASecondItem_DeselectsTheFirst()
    {
        var (control, parameter) = RequiredSideControl();
        var viewModel = new ListControlViewModel(control, parameter);
        viewModel.Items[0].IsSelected = true;

        viewModel.Items[1].IsSelected = true;

        viewModel.Items[0].IsSelected.Should().BeFalse();
        viewModel.Items[1].IsSelected.Should().BeTrue();
    }

    [Fact]
    public void NoSelection_OnRequiredControl_HasErrorsTrue()
    {
        var (control, parameter) = RequiredSideControl();
        var viewModel = new ListControlViewModel(control, parameter);

        viewModel.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void MakingASelection_OnRequiredControl_ClearsErrors()
    {
        var (control, parameter) = RequiredSideControl();
        var viewModel = new ListControlViewModel(control, parameter);

        viewModel.Items[0].IsSelected = true;

        viewModel.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void EmptySlider_HasEmptySelectionWithoutThrowing()
    {
        var viewModel = new ListControlViewModel(new Slider_t("Empty"));

        viewModel.SelectedValue.Should().BeNull();
        viewModel.Text.Should().BeNull();
    }
}
