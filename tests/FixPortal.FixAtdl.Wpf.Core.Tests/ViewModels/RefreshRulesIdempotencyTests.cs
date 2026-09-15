using System.Globalization;
using System.Reflection;
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

        // RefreshRules is private; EditViewModel normally re-enters it when a control value changes.
        // Idempotency under the "nothing changed" re-entry is load-bearing for the fixpoint loop's
        // termination argument, so drive a second and third converged refresh directly.
        RefreshRulesAgain(model);
        RefreshRulesAgain(model);

        Capture(model).Should().Be(before);
    }

    private static void RefreshRulesAgain(EditViewModel model) =>
        typeof(EditViewModel)
            .GetMethod("RefreshRules", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(model, null);

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
