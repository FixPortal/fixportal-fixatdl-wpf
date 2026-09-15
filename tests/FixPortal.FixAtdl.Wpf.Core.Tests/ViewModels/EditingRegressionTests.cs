using System.Globalization;
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
    public void ReverseOrderedValueChain_ConvergesAndPropagatesEveryValue()
    {
        // Audit H1: the fixpoint pass bound in the EditViewModel refresh loop is the greater of 64
        // passes and four passes per state rule. With 1-2 rules the 64-pass floor always wins, so a
        // regression in the per-rule multiplier is invisible; this chain forces the scaling term to
        // carry the load.
        //
        // Chain construction (per the audit's Phase-3 constraint): chain link i carries a state rule
        // that fires when link i-1 holds the value i-1 and then writes i to link i. The controls are
        // added to the panel from link N down to link 0, so the rules are applied in reverse dependency
        // order: within one pass, a rule whose condition has just become true has already been
        // evaluated, so exactly one rule fires per pass. (Had application order matched dependency
        // order, the whole chain would cascade to fixpoint within a single pass - two passes regardless
        // of N - and the test would pass even at multiplier zero.) Every value write raises a control
        // property change that sets the refresh-pending flag, and the pass bound is checked before the
        // loop-exit test, so convergence costs N propagation passes plus one quiescence pass: N + 1 in
        // total.
        //
        // N = 100: the bound evaluates to 400 on current code (101 passes against 400 converges), but
        // only 100 if the per-rule multiplier is regressed from four to one (101 exceeds 100, so the
        // quiescence pass trips "State rules did not converge"). N sits above the 64-pass floor, so the
        // multiplier alone is what this test guards.
        const int ruleCount = 100;
        var strategy = new Strategy_t();
        var panel = new StrategyPanel_t(strategy);
        strategy.StrategyLayout = new StrategyLayout_t { StrategyPanel = panel };

        for (var i = ruleCount; i >= 0; i--)
        {
            var link = new SingleSpinner_t($"K{i}");
            if (i > 0)
            {
                link.StateRules.Add(
                    new StateRule_t
                    {
                        Value = i.ToString(CultureInfo.InvariantCulture),
                        Edit = new Edit_t<Control_t>
                        {
                            Field = $"K{i - 1}",
                            Operator = Operator_t.Equal,
                            Value = (i - 1).ToString(CultureInfo.InvariantCulture),
                        },
                    }
                );
            }

            panel.Controls.Add(link);
        }

        strategy.Controls["K0"].SetValue(0m);
        var model = new EditViewModel(strategy);

        model.StrategyErrors.Should().BeEmpty();
        model.HasErrors.Should().BeFalse();
        for (var i = 0; i <= ruleCount; i++)
        {
            model.Controls.Single(control => control.Id == $"K{i}").Value.Should().Be((decimal)i);
        }
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
