using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Elements.Support;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Model.Types;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

public class ReviewRegressionTests
{
    [Fact]
    public void ReadBack_IncludesConstantAndLoadedParametersWithoutControls()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Controls["Qty"].SetValue(1m);
        var constant = new Parameter_t<String_t>("Constant") { FixTag = 9001 };
        constant.Value.ConstValue = "fixed";
        strategy.Parameters.Add(constant);
        strategy.Parameters.Add(new Parameter_t<String_t>("Loaded") { FixTag = 9002, WireValue = "loaded" });
        strategy.Parameters.Add(new Parameter_t<String_t>("Empty") { FixTag = 9003 });
        var model = new EditViewModel(strategy);

        model.HasErrors.Should().BeFalse();
        model.ReadBackFixValues().Should().Contain(9001, "fixed").And.Contain(9002, "loaded").And.NotContainKey(9003);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvalidParameterWithoutControl_BlocksBothReadBackPaths(bool invalidConstant)
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Controls["Qty"].SetValue(1m);
        var parameter = new Parameter_t<Int_t>("Uneditable") { FixTag = 9001, Use = Use_t.Required };
        parameter.Value.MinValue = 0;
        if (invalidConstant)
        {
            parameter.Value.ConstValue = -1;
        }
        strategy.Parameters.Add(parameter);
        var model = new EditViewModel(strategy);

        model.HasErrors.Should().BeTrue();
        model.StrategyErrors.Should().NotBeEmpty();
        var direct = () => model.ReadBackFixValues();
        var group = () => model.ReadBackStrategyParametersGrp();
        direct.Should().Throw<InvalidOperationException>();
        group.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DuplicateParameterTags_AreRejectedAtConstruction()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Parameters["Qty"].FixTag = 9001;
        strategy.Parameters.Add(new Parameter_t<String_t>("Other") { FixTag = 9001 });
        var create = () => new EditViewModel(strategy);

        create.Should().Throw<ArgumentException>().WithMessage("*9001*");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void NumericSlider_FallbackBoundsHaveWidthAtPivots(bool minimumOnly)
    {
        var parameter = new Parameter_t<Float_t>("Limit");
        parameter.Value.MinValue = minimumOnly ? 100 : null;
        parameter.Value.MaxValue = minimumOnly ? null : 0;
        var model = new ControlViewModel(new Slider_t("Limit"), parameter);

        model.NumericMaximum.Should().BeGreaterThan(model.NumericMinimum);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WrongValueType_IsReportedThroughValidation(bool list)
    {
        Control_t control = list ? new DropDownList_t("Input") : new TextField_t("Input");
        var model = new ControlViewModel(control);
        var edit = () => model.Value = 5;

        edit.Should().NotThrow();
        model.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void LoadedEnumMissingFromList_HasSafeDisplayText()
    {
        var control = new DropDownList_t("List");
        control.ListItems.Add(new ListItem_t { EnumId = "A", UiRep = "Alpha" });
        var parameter = new Parameter_t<String_t>("List");
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "A", WireValue = "alpha" });
        parameter.EnumPairs.Add(new EnumPair_t { EnumId = "B", WireValue = "beta" });
        parameter.WireValue = "beta";
        control.SetValueFromParameter(parameter);
        var model = new ListControlViewModel(control, parameter);

        model.Text.Should().Be("B");
    }

    [Fact]
    public void ThrowingRule_DoesNotSkipLaterRulesOrStrategyEdits()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Controls["Qty"].SetValue(12m);
        var list = new DropDownList_t("List");
        list.ListItems.Add(new ListItem_t { EnumId = "A", UiRep = "Alpha" });
        list.StateRules.Add(
            new StateRule_t
            {
                Value = "unknown",
                Edit = new Edit_t<Control_t> { Field = "Qty", Operator = Operator_t.Exist },
            }
        );
        var later = new TextField_t("Later");
        later.StateRules.Add(
            new StateRule_t
            {
                Enabled = false,
                Edit = new Edit_t<Control_t> { Field = "Qty", Operator = Operator_t.Exist },
            }
        );
        strategy.StrategyLayout.StrategyPanel.Controls.Add(list);
        strategy.StrategyLayout.StrategyPanel.Controls.Add(later);
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

        model.Controls[2].Enabled.Should().BeFalse();
        model.StrategyErrors.Should().Contain("Quantity must exceed 20").And.HaveCount(2);
        model.Controls[0].Value = 21m;
        model.StrategyErrors.Should().NotContain("Quantity must exceed 20").And.ContainSingle();
    }
}
