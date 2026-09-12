using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Model.Types;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

public class ConformanceTests
{
    [Theory]
    [InlineData("XNYS XNAS")]
    [InlineData(null)]
    public void AmendmentList_PreservesLoadedSelectionsOrAbsenceInsteadOfDefaults(string? wireValue)
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Controls["Qty"].SetValue(12m);
        var parameter = new Parameter_t<MultipleStringValue_t>("Venues") { FixTag = 9001 };
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "A", WireValue = "XNYS" });
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "B", WireValue = "XNAS" });
        if (wireValue is not null)
        {
            parameter.WireValue = wireValue;
        }
        var control = new CheckBoxList_t("Venues") { ParameterRef = "Venues", InitValue = "A" };
        control.ListItems.Add(new ListItem_t { EnumId = "A", UiRep = "New York" });
        control.ListItems.Add(new ListItem_t { EnumId = "B", UiRep = "Nasdaq" });
        control.SetValueFromParameter(parameter);
        strategy.Parameters.Add(parameter);
        strategy.StrategyLayout.StrategyPanel.Controls.Add(control);

        var model = new EditViewModel(strategy);

        var list = model.Controls[1].Should().BeOfType<ListControlViewModel>().Subject;
        list.Items.Count(item => item.IsSelected).Should().Be(wireValue is null ? 0 : 2);
        model.ReadBackFixValues().GetValueOrDefault(9001).Should().Be(wireValue);
    }

    // FIXatdl 1.1, state-rule conventions i–iv: initial false inverts presentation only.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InitiallyFalseRule_InvertsPresentationAndPreservesLoadedValue(bool state)
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Parameters["Qty"].FixTag = 9000;
        var qty = strategy.Controls["Qty"];
        qty.SetValue(12m);
        strategy.StrategyLayout.StrategyPanel.Controls.Add(new CheckBox_t("Toggle"));
        qty.StateRules.Add(
            new StateRule_t
            {
                Enabled = state,
                Visible = state,
                Value = "{NULL}",
                Edit = new Edit_t<Control_t>
                {
                    Field = "Toggle",
                    Operator = Operator_t.Equal,
                    Value = "true",
                },
            }
        );

        var model = new EditViewModel(strategy);

        model.Controls[0].Enabled.Should().Be(!state);
        model.Controls[0].Visible.Should().Be(!state);
        model.Controls[0].Value.Should().Be(12m);
        model.HasErrors.Should().BeFalse();
        model.ReadBackFixValues()[9000].Should().Be("12");
        model.Controls[1].Value = true;
        model.Controls[0].Value.Should().BeNull();
        model.Controls[1].Value = false;
        model.Controls[0].Value.Should().Be(12m);
    }

    // Control/@parameterRef and checkedEnumRef/uncheckedEnumRef map controls to one parameter.
    [Fact]
    public void ComplementaryRadios_SharingAParameterEmitOneTagAfterEachSelection()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Controls["Qty"].SetValue(12m);
        var parameter = new Parameter_t<String_t>("Side") { FixTag = 9001, Use = Use_t.Required };
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "Buy", WireValue = "1" });
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "Sell", WireValue = "2" });
        strategy.Parameters.Add(parameter);
        var buy = new RadioButton_t("Buy")
        {
            ParameterRef = "Side",
            RadioGroup = "Side",
            CheckedEnumRef = "Buy",
            UncheckedEnumRef = "Sell",
        };
        var sell = new RadioButton_t("Sell")
        {
            ParameterRef = "Side",
            RadioGroup = "Side",
            CheckedEnumRef = "Sell",
            UncheckedEnumRef = "Buy",
        };
        buy.SetValue(true);
        strategy.StrategyLayout.StrategyPanel.Controls.Add(buy);
        strategy.StrategyLayout.StrategyPanel.Controls.Add(sell);
        var model = new EditViewModel(strategy);

        model.ReadBackFixValues().Should().ContainSingle().Which.Should().Be(new KeyValuePair<int, string>(9001, "1"));
        model.Controls[2].Value = true;
        model.Controls[1].Value.Should().Be(false);
        model.ReadBackFixValues()[9001].Should().Be("2");
        model.Controls[1].Value = true;
        model.Controls[2].Value.Should().Be(false);
        model.ReadBackFixValues()[9001].Should().Be("1");
    }
}
