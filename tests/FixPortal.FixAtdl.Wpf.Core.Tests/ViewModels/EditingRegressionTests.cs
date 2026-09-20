using AwesomeAssertions;
using FixPortal.FixAtdl.Fix;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Elements.Support;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Model.Types;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

public class EditingRegressionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnknownEnumInStateRule_IsValidatedAndCanRecover(bool editable)
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Controls["Qty"].SetValue(12m);
        var toggle = new CheckBox_t("Toggle");
        FixPortal.FixAtdl.Model.Controls.Support.ListControlBase list = editable
            ? new EditableDropDownList_t("List")
            : new DropDownList_t("List");
        list.ListItems.Add(new ListItem_t { EnumId = "A", UiRep = "Alpha" });
        list.StateRules.Add(
            new StateRule_t
            {
                Value = "unknown",
                Edit = new Edit_t<Control_t>
                {
                    Field = "Toggle",
                    Operator = Operator_t.Equal,
                    Value = "true",
                },
            }
        );
        strategy.StrategyLayout.StrategyPanel.Controls.Add(toggle);
        strategy.StrategyLayout.StrategyPanel.Controls.Add(list);
        var model = new EditViewModel(strategy);
        var activate = () => model.Controls[1].Value = true;

        activate.Should().NotThrow();
        model.HasErrors.Should().Be(!editable);
        var listModel = model.Controls[2].Should().BeOfType<ListControlViewModel>().Subject;
        listModel.Text.Should().Be(editable ? "unknown" : null);
        model.Controls[0].Value = 13m;
        model.HasErrors.Should().Be(!editable);
        model.Controls[1].Value = false;
        model.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void CyclicNullRule_BlocksSubmissionWithoutRecursing()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        var control = strategy.Controls["Qty"];
        control.SetValue(12m);
        control.StateRules.Add(
            new StateRule_t
            {
                Value = "{NULL}",
                Edit = new Edit_t<Control_t> { Field = "Qty", Operator = Operator_t.Exist },
            }
        );
        var model = new EditViewModel(strategy);
        model.HasErrors.Should().BeTrue();
        model.StrategyErrors.Should().ContainSingle().Which.Should().Contain("cyclic");
    }

    [Fact]
    public void Construction_PreservesAnExplicitlyClearedValue()
    {
        var (control, parameter) = TestControls.RequiredNumericControl();
        control.InitValue = 12m;
        control.SetValue(null!);
        var model = new ControlViewModel(control, parameter);
        model.Value.Should().BeNull();
        model.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void StateRules_DisableHideClearAndRestoreOnTransitions()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        var qty = strategy.Controls["Qty"];
        qty.SetValue(12m);
        var toggle = new CheckBox_t("Toggle");
        strategy.StrategyLayout.StrategyPanel.Controls.Add(toggle);
        qty.StateRules.Add(
            new StateRule_t
            {
                Enabled = false,
                Visible = false,
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

        model.Controls[1].Value = true;
        model.Controls[0].Enabled.Should().BeFalse();
        model.Controls[0].Visible.Should().BeFalse();
        model.Controls[0].Value.Should().BeNull();
        model.Controls[1].Value = false;
        model.Controls[0].Enabled.Should().BeTrue();
        model.Controls[0].Visible.Should().BeTrue();
        model.Controls[0].Value.Should().Be(12m);
    }

    [Fact]
    public void MultipleNullStateRules_RestoreTheOriginalValueOnce()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        var qty = strategy.Controls["Qty"];
        qty.SetValue(12m);
        var toggle = new CheckBox_t("Toggle");
        strategy.StrategyLayout.StrategyPanel.Controls.Add(toggle);

        for (var i = 0; i < 2; i++)
        {
            qty.StateRules.Add(
                new StateRule_t
                {
                    Value = "{NULL}",
                    Edit = new Edit_t<Control_t>
                    {
                        Field = "Toggle",
                        Operator = Operator_t.Equal,
                        Value = "true",
                    },
                }
            );
        }

        var model = new EditViewModel(strategy);

        model.Controls[1].Value = true;
        model.Controls[0].Value.Should().BeNull();
        model.Controls[1].Value = false;

        model.Controls[0].Value.Should().Be(12m);
    }

    [Fact]
    public void StrategyEdits_BlockReadBackUntilSatisfied()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Controls["Qty"].SetValue(12m);
        strategy.StrategyEdits.Add(
            new StrategyEdit_t
            {
                ErrorMessage = "Quantity must exceed 20",
                Edit = new Edit_t<IParameter>
                {
                    Field = "Qty",
                    Operator = Operator_t.GreaterThan,
                    Value = "20",
                },
            }
        );
        var model = new EditViewModel(strategy);

        model.HasErrors.Should().BeTrue();
        var read = () => model.ReadBackFixValues();
        read.Should().Throw<InvalidOperationException>();
        model.Controls[0].Value = 21m;
        model.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void EditableList_SupportsFreeTextAndExistingSelection()
    {
        var control = new EditableDropDownList_t("Text");
        control.ListItems.Add(new ListItem_t { EnumId = "A", UiRep = "Alpha" });
        var parameter = new Parameter_t<String_t>("Text");
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "A", WireValue = "WIRE" });
        var model = new ListControlViewModel(control, parameter) { Text = "custom" };
        parameter.WireValue.Should().Be("custom");
        model.Text = "Alpha";
        model.SelectedValue.Should().Be("A");
        parameter.WireValue.Should().Be("WIRE");
    }

    [Fact]
    public void ListEditing_PreservesLoadedValuesAndSerializesMultipleSelections()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        var control = new CheckBoxList_t("Venues") { ParameterRef = "Venues", InitValue = "A" };
        control.ListItems.Add(new ListItem_t { EnumId = "A", UiRep = "Alpha" });
        control.ListItems.Add(new ListItem_t { EnumId = "B", UiRep = "Beta" });
        var parameter = new Parameter_t<MultipleStringValue_t>("Venues") { FixTag = 9001 };
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "A", WireValue = "XNYS" });
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "B", WireValue = "XNAS" });
        strategy.Parameters.Add(parameter);
        strategy.StrategyLayout.StrategyPanel.Controls.Add(control);
        control.LoadInitValue(FixFieldValueProvider.Empty);
        var model = new EditViewModel(strategy);
        model.Controls[0].Value = 1m;
        var list = model.Controls[1].Should().BeOfType<ListControlViewModel>().Subject;

        list.Items[1].IsSelected = true;

        list.Items.Should().OnlyContain(item => item.IsSelected);
        model.ReadBackFixValues()[9001].Should().Be("XNYS XNAS");
    }

    [Theory]
    [InlineData(true, "Y")]
    [InlineData(false, "N")]
    public void ReadBack_UsesBooleanWireFormat(bool value, string expected)
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.StrategyLayout.StrategyPanel.Controls.Add(new CheckBox_t("Flag") { ParameterRef = "Flag" });
        strategy.Parameters.Add(new Parameter_t<Boolean_t>("Flag") { FixTag = 9002 });
        var model = new EditViewModel(strategy);
        model.Controls[0].Value = 1m;
        model.Controls[1].Value = value;

        model.ReadBackFixValues()[9002].Should().Be(expected);
    }

    [Fact]
    public void InvalidNumericInput_IsAValidationError()
    {
        var (control, parameter) = TestControls.RequiredNumericControl();
        var model = new ControlViewModel(control, parameter) { Value = 12m };

        var edit = () => model.Value = "bad";

        edit.Should().NotThrow();
        model.HasErrors.Should().BeTrue();
    }
}
