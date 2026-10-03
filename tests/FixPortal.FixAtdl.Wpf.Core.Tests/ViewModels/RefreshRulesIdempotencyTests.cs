using System.Globalization;
using AwesomeAssertions;
using FixPortal.FixAtdl.Fix;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Elements.Support;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

public class RefreshRulesIdempotencyTests
{
    [Fact]
    public void SecondAndThirdRefreshRules_AfterConvergence_ProduceZeroSideEffects()
    {
        var strategy = TestControls.MinimalStrategyWithOneRequiredControl();
        strategy.Controls["Qty"].SetValue(12m);
        var toggle = new CheckBox_t("Toggle");
        strategy.StrategyLayout.StrategyPanel.Controls.Add(toggle);
        strategy.StrategyLayout.StrategyPanel.Controls.Add(new CheckBox_t("Refresh"));
        strategy
            .Controls["Qty"]
            .StateRules.Add(
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
        model.Controls[1].Value = true;

        model.Controls[0].Enabled.Should().BeFalse();
        model.Controls[0].Visible.Should().BeFalse();
        model.Controls[0].Value.Should().BeNull();
        model.HasErrors.Should().BeTrue();

        var before = Capture(model);

        // A value change on an unrelated control re-enters RefreshRules through the public surface.
        model.Controls[2].Value = true;
        model.Controls[2].Value = false;

        Capture(model).Should().Be(before);
    }

    private static string Capture(EditViewModel model)
    {
        var lines = model
            .Controls.Select(control =>
                string.Join(
                    '|',
                    control.Id,
                    Convert.ToString(control.Value, CultureInfo.InvariantCulture) ?? "<null>",
                    control.Enabled,
                    control.Visible,
                    control.Visibility,
                    control.HasErrors,
                    string.Join(",", control.GetErrors(null).Select(error => error.ErrorMessage))
                )
            )
            .ToList();
        lines.Add($"HasErrors={model.HasErrors}");
        lines.Add($"StrategyErrors={string.Join(";", model.StrategyErrors)}");
        return string.Join('\n', lines);
    }
}
