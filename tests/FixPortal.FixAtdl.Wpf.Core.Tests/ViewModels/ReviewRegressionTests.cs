using AwesomeAssertions;
using FixPortal.FixAtdl.Diagnostics.Exceptions;
using FixPortal.FixAtdl.Fix;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Elements.Support;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Model.Types;
using FixPortal.FixAtdl.Validation;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;
using NSubstitute;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

public class ReviewRegressionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InternalParameterFailure_IsNotReportedAsUserInputError(bool withoutControl)
    {
        var parameter = Substitute.For<IParameter>();
        parameter.Name.Returns("Broken");
        if (withoutControl)
        {
            parameter.WireValue.Returns(_ => throw new InternalErrorException("Broken parameter invariant"));
        }
        else
        {
            parameter
                .SetValueFromControl(Arg.Any<Control_t>())
                .Returns(_ => throw new InternalErrorException("Broken parameter invariant"));
        }

        Action create = () =>
        {
            if (withoutControl)
            {
                var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
                strategy.Parameters.Add(parameter);
                _ = new EditViewModel(strategy);
            }
            else
            {
                _ = new ControlViewModel(new TextField_t("Input"), parameter);
            }
        };
        create.Should().Throw<InternalErrorException>().WithMessage("Broken parameter invariant");
    }

    /// <summary>
    /// A strategy with the usual required "Qty" control plus a second control bound to a substituted
    /// parameter that starts healthy, so construction succeeds and only the later edit fails.
    /// </summary>
    private static (Strategy_t Strategy, Action Break) BrokenOnDemand()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        var control = new TextField_t("Broken") { ParameterRef = "Broken" };
        strategy.StrategyLayout.StrategyPanel.Controls.Add(control);

        bool failing = false;
        var parameter = Substitute.For<IParameter>();
        parameter.Name.Returns("Broken");
        parameter.FixTag.Returns(new FixTag(9100));
        parameter.IsSet.Returns(true);
        parameter.WireValue.Returns("old");
        parameter
            .SetValueFromControl(Arg.Any<Control_t>())
            .Returns(_ =>
                failing ? throw new InternalErrorException("Broken parameter invariant") : ValidationResult.ValidResult
            );
        strategy.Parameters.Add(parameter);

        return (strategy, () => failing = true);
    }

    [Fact]
    public void InternalFailureDuringAnEdit_BlocksReadBackRatherThanEmittingTheStaleWireValue()
    {
        // Validation writes the control before the parameter, so an InternalErrorException escaping it
        // leaves the two disagreeing with nothing recorded in the error dictionary. Without the
        // torn-write latch HasErrors stayed false here and ReadBackFixValues emitted the parameter's
        // OLD wire value for tag 9100 - a silently wrong order value.
        var (strategy, breakIt) = BrokenOnDemand();
        var model = new EditViewModel(strategy);
        model.Controls.Single(control => control.UnderlyingControl.Id == "Qty").Value = 1m;

        // Baseline must be clean, or the assertions below would hold whatever the code does.
        model.HasErrors.Should().BeFalse();
        model.ReadBackFixValues().Should().ContainKey(9100);

        breakIt();
        var edit = () => model.Controls.Single(control => control.UnderlyingControl.Id == "Broken").Value = "abc";

        edit.Should().Throw<InternalErrorException>();
        model.HasErrors.Should().BeTrue("the control took the value and its parameter did not");
        var read = () => model.ReadBackFixValues();
        read.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void InternalFailureDuringAStateRule_BlocksReadBackRatherThanEmittingTheStaleWireValue()
    {
        var (strategy, breakIt) = BrokenOnDemand();
        var toggle = new CheckBox_t("Toggle");
        strategy.StrategyLayout.StrategyPanel.Controls.Add(toggle);
        strategy
            .Controls["Broken"]
            .StateRules.Add(
                new StateRule_t
                {
                    Value = "abc",
                    Edit = new Edit_t<Control_t>
                    {
                        Field = "Toggle",
                        Operator = Operator_t.Equal,
                        Value = "true",
                    },
                }
            );
        var model = new EditViewModel(strategy);
        breakIt();

        var edit = () => model.Controls.Single(control => control.UnderlyingControl.Id == "Toggle").Value = true;

        edit.Should().Throw<InternalErrorException>();
        model.HasErrors.Should().BeTrue();
        var read = () => model.ReadBackFixValues();
        read.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void InternalFailureAfterAnExistingValidationError_RetainsTheErrorStore()
    {
        var parameter = Substitute.For<IParameter>();
        parameter.Name.Returns("Input");
        var calls = 0;
        parameter
            .SetValueFromControl(Arg.Any<Control_t>())
            .Returns(_ =>
                calls++ == 0
                    ? new ValidationResult(ValidationResult.ResultType.Invalid, "already invalid")
                    : throw new InternalErrorException("Broken parameter invariant")
            );
        var model = new ControlViewModel(new TextField_t("Input"), parameter);

        model.HasErrors.Should().BeTrue();
        var edit = () => model.Value = "changed";

        edit.Should().Throw<InternalErrorException>();
        model.HasErrors.Should().BeTrue();
        model.GetErrors(nameof(ControlViewModel.Value)).Should().NotBeEmpty();
    }

    [Fact]
    public void InternalFailureInTheParameterSweep_DoesNotLatchTheRefreshGuard()
    {
        // RefreshRules' finally reads every control-less parameter's WireValue BEFORE clearing
        // _refreshing. An exception escaping that sweep skipped the reset, and since nothing else
        // writes the flag, every later refresh returned at the top - rules silently stopped applying
        // for the lifetime of the view model.
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        Exception? thrown = null;
        var orphan = Substitute.For<IParameter>();
        orphan.Name.Returns("Orphan");
        orphan.WireValue.Returns(_ => thrown is null ? "ok" : throw thrown);
        strategy.Parameters.Add(orphan);

        var model = new EditViewModel(strategy);
        var qty = model.Controls.Single(control => control.UnderlyingControl.Id == "Qty");

        // An exception the sweep does not catch, escaping through the finally.
        thrown = new InternalErrorException("Broken parameter invariant");
        var poison = () => qty.Value = 1m;
        poison.Should().Throw<InternalErrorException>();

        // Now one the sweep DOES catch and record. It can only reach StrategyErrors if a refresh
        // still runs, which is exactly what the latched guard would have prevented.
        thrown = new FormatException("orphan wire value is malformed");
        qty.Value = 2m;

        model.StrategyErrors.Should().ContainSingle().Which.Should().Be("orphan wire value is malformed");
    }

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
