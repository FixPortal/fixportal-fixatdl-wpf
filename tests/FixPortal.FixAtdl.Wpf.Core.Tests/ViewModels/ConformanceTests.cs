using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Model.Types;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

public class ConformanceTests
{
    [Fact]
    public void InvertedList_WithoutInitializationRemainsAbsentUntilEdited()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Controls["Qty"].SetValue(12m);
        var parameter = new Parameter_t<MultipleStringValue_t>("Venues") { FixTag = 9001 };
        parameter.Value.InvertOnWire = true;
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "A", WireValue = "XNYS" });
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "B", WireValue = "XNAS" });
        strategy.Parameters.Add(parameter);
        var list = new CheckBoxList_t("Venues") { ParameterRef = "Venues" };
        list.ListItems.Add(new ListItem_t { EnumId = "A", UiRep = "New York" });
        list.ListItems.Add(new ListItem_t { EnumId = "B", UiRep = "Nasdaq" });
        strategy.StrategyLayout.StrategyPanel.Controls.Add(list);
        var model = new EditViewModel(strategy);

        model.ReadBackFixValues().Should().NotContainKey(9001);
        var listModel = (ListControlViewModel)model.Controls[1];
        listModel.Items[0].IsSelected = true;
        model.ReadBackFixValues()[9001].Should().Be("XNAS");
        listModel.Items[0].IsSelected = false;
        model.ReadBackFixValues()[9001].Should().Be("XNYS XNAS");
    }

    [Fact]
    public void InvertedList_ClearAndRestoreKeepsExplicitNullDistinctFromEmptySelection()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Controls["Qty"].SetValue(12m);
        var parameter = new Parameter_t<MultipleStringValue_t>("Venues") { FixTag = 9001 };
        parameter.Value.InvertOnWire = true;
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "A", WireValue = "XNYS" });
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "B", WireValue = "XNAS" });
        strategy.Parameters.Add(parameter);
        var list = new CheckBoxList_t("Venues") { ParameterRef = "Venues", InitValue = "A" };
        list.ListItems.Add(new ListItem_t { EnumId = "A", UiRep = "New York" });
        list.ListItems.Add(new ListItem_t { EnumId = "B", UiRep = "Nasdaq" });
        list.StateRules.Add(
            new StateRule_t
            {
                Value = "{NULL}",
                Edit = new Edit_t<Control_t>
                {
                    Field = "Clear",
                    Operator = Operator_t.Equal,
                    Value = "true",
                },
            }
        );
        strategy.StrategyLayout.StrategyPanel.Controls.Add(list);
        strategy.StrategyLayout.StrategyPanel.Controls.Add(new CheckBox_t("Clear"));
        var model = new EditViewModel(strategy);

        model.ReadBackFixValues()[9001].Should().Be("XNAS");
        model.Controls[2].Value = true;
        model.ReadBackFixValues().Should().NotContainKey(9001);
        model.Controls[2].Value = false;
        model.ReadBackFixValues()[9001].Should().Be("XNAS");
        var listModel = (ListControlViewModel)model.Controls[1];
        listModel.Items[0].IsSelected = false;
        model.ReadBackFixValues()[9001].Should().Be("XNYS XNAS");
        listModel.Items[1].IsSelected = true;
        model.ReadBackFixValues()[9001].Should().Be("XNYS");
    }

    [Theory]
    [InlineData(0, Use_t.Required)]
    [InlineData(1, Use_t.Required)]
    [InlineData(0, Use_t.Optional)]
    [InlineData(1, Use_t.Optional)]
    public void SharedRadioParameter_UsesSelectedValueAndValidatesTheWholeGroup(int selected, Use_t use)
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Controls["Qty"].SetValue(12m);
        var parameter = new Parameter_t<String_t>("Side") { FixTag = 9001, Use = use };
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "A", WireValue = "1" });
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "B", WireValue = "2" });
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "None", WireValue = "{NULL}" });
        strategy.Parameters.Add(parameter);
        foreach (var id in new[] { "A", "B" })
        {
            var radio = new RadioButton_t(id)
            {
                ParameterRef = "Side",
                RadioGroup = "Side",
                CheckedEnumRef = id,
                UncheckedEnumRef = "None",
            };
            radio.SetValue((selected == 0 ? "A" : "B") == id);
            strategy.StrategyLayout.StrategyPanel.Controls.Add(radio);
        }
        var model = new EditViewModel(strategy);

        model.HasErrors.Should().BeFalse();
        model.ReadBackFixValues()[9001].Should().Be(selected == 0 ? "1" : "2");
        var other = selected == 0 ? 2 : 1;
        model.Controls[other].Value = true;
        model.Controls[selected + 1].Value.Should().Be(false);
        model.HasErrors.Should().BeFalse();
        model.ReadBackFixValues()[9001].Should().Be(selected == 0 ? "2" : "1");
        model.Controls[other].Value = false;
        model.HasErrors.Should().Be(use == Use_t.Required);
        if (use == Use_t.Optional)
        {
            model.ReadBackFixValues().Should().NotContainKey(9001);
        }
    }

    [Fact]
    public void ImmutableSelectedRadio_PreventsSelectingItsMutableSiblingOnAmendment()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Controls["Qty"].SetValue(12m);
        var fixedRadio = new RadioButton_t("Fixed") { ParameterRef = "Fixed", RadioGroup = "Mode" };
        fixedRadio.SetValue(true);
        strategy.StrategyLayout.StrategyPanel.Controls.Add(fixedRadio);
        strategy.StrategyLayout.StrategyPanel.Controls.Add(
            new RadioButton_t("Other") { ParameterRef = "Other", RadioGroup = "Mode" }
        );
        strategy.Parameters.Add(new Parameter_t<Boolean_t>("Fixed") { MutableOnCxlRpl = false });
        strategy.Parameters.Add(new Parameter_t<Boolean_t>("Other"));
        var model = new EditViewModel(strategy, true);

        model.Controls[2].Value = true;

        model.Controls[1].Value.Should().Be(true);
        model.Controls[2].Value.Should().Be(false);
        model.Controls[2].Enabled.Should().BeFalse();
    }

    [Fact]
    public void ImmutableAmendmentList_RejectsItemAndSelectionMutations()
    {
        var control = new DropDownList_t("Side");
        control.ListItems.Add(new ListItem_t { EnumId = "A", UiRep = "Alpha" });
        control.ListItems.Add(new ListItem_t { EnumId = "B", UiRep = "Beta" });
        var parameter = new Parameter_t<String_t>("Side") { MutableOnCxlRpl = false };
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "A", WireValue = "a" });
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "B", WireValue = "b" });
        parameter.WireValue = "a";
        control.SetValueFromParameter(parameter);
        var model = new ListControlViewModel(control, parameter, true);

        model.Items[1].IsSelected = true;
        model.Items[1].IsSelected.Should().BeFalse();
        model.Items[0].IsSelected.Should().BeTrue();
        model.SelectedValue = "B";
        model.SelectedValue.Should().Be("A");
        parameter.WireValue.Should().Be("a");
        model.Enabled.Should().BeFalse();
    }

    [Fact]
    public void NumericSlider_UsesNumericValuesAndPreservesExplicitNull()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Controls["Qty"].SetValue(12m);
        var slider = new Slider_t("Limit") { ParameterRef = "Limit" };
        strategy.StrategyLayout.StrategyPanel.Controls.Add(slider);
        strategy.Parameters.Add(new Parameter_t<Float_t>("Limit") { FixTag = 9001 });
        var model = new EditViewModel(strategy);
        model.Controls[1].Should().BeOfType<ControlViewModel>();
        model.Controls[1].Value.Should().BeNull();
        model.ReadBackFixValues().Should().NotContainKey(9001);
        model.Controls[1].Value = 12.5m;
        model.ReadBackFixValues()[9001].Should().Be("12.5");
        model.Controls[1].Value = null;
        model.ReadBackFixValues().Should().NotContainKey(9001);
    }

    [Theory]
    [InlineData(false, 25)]
    [InlineData(true, 12)]
    public void ImmutableParameter_CanOnlyChangeForNewOrders(bool isAmendment, int expected)
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Parameters["Qty"].FixTag = 9000;
        strategy.Parameters["Qty"].MutableOnCxlRpl = false;
        strategy.Controls["Qty"].SetValue(12m);
        var trigger = new CheckBox_t("Trigger");
        strategy.StrategyLayout.StrategyPanel.Controls.Add(trigger);
        strategy
            .Controls["Qty"]
            .StateRules.Add(
                new StateRule_t
                {
                    Enabled = true,
                    Value = "25",
                    Edit = new Edit_t<Control_t>
                    {
                        Field = "Trigger",
                        Operator = Operator_t.Equal,
                        Value = "true",
                    },
                }
            );
        var model = new EditViewModel(strategy, isAmendment);
        model.Controls[0].Value = 20m;
        model.Controls[0].Value.Should().Be(isAmendment ? 12m : 20m);
        model.Controls[1].Value = true;
        model.Controls[0].Enabled.Should().Be(!isAmendment);
        model.Controls[0].Value.Should().Be((decimal)expected);
        model
            .ReadBackFixValues()[9000]
            .Should()
            .Be(expected.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

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
