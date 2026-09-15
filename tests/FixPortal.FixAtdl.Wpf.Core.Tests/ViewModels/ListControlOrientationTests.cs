using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

public class ListControlOrientationTests
{
    [Theory]
    [InlineData(Orientation_t.Vertical, "Vertical")]
    [InlineData(Orientation_t.Horizontal, "Horizontal")]
    [InlineData(null, "Horizontal")]
    public void Orientation_MapsModelOrientationToXamlString(Orientation_t? orientation, string expected)
    {
        var control = new CheckBoxList_t("Sides") { Orientation = orientation };
        control.ListItems.Add(new ListItem_t { EnumId = "1", UiRep = "Buy" });
        control.ListItems.Add(new ListItem_t { EnumId = "2", UiRep = "Sell" });
        var viewModel = new ListControlViewModel(control);

        viewModel.Orientation.Should().Be(expected);
    }
}
