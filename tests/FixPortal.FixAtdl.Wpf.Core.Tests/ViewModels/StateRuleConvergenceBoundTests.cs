using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

public class StateRuleConvergenceBoundTests
{
    // Pass arithmetic: with N rules in a reverse-ordered value chain, each pass arms exactly one
    // control, so the loop needs N propagation passes plus one quiescence pass before _refreshPending
    // stays false — N+1 = 65 passes for N = 64. The ×4 multiplier gives a bound of Max(64, 256), which
    // converges; a regression to ×1 tightens it to Max(64, 64) = 64, so the 65th (quiescence) pass
    // trips the bound check and falsely reports "State rules did not converge". N must be ≥ 64 for
    // N+1 > Max(64, N) to hold; anything smaller never approaches the bound even at multiplier ×1.
    [Fact]
    public void ReverseOrderedValueChain_Of64Rules_ConvergesWithoutBoundError()
    {
        const int ruleCount = 64;
        var strategy = new Strategy_t();
        var panel = new StrategyPanel_t(strategy);
        strategy.StrategyLayout = new StrategyLayout_t { StrategyPanel = panel };

        // Rule i (attached to control C{i}) fires when C{i-1} == "armed" and arms C{i}. Adding the
        // controls in reverse index order makes the rule application order the opposite of the
        // dependency direction, so pass k arms only C{k}; the dependency order (C0 first) would
        // cascade the whole chain to fixpoint within a single pass (2 passes), regardless of N,
        // and could never detect a bound regression.
        for (var i = ruleCount; i >= 0; i--)
        {
            var control = new TextField_t($"C{i}");
            control.SetValue(i == 0 ? "armed" : "idle");
            if (i > 0)
            {
                control.StateRules.Add(
                    new StateRule_t
                    {
                        Value = "armed",
                        Edit = new Edit_t<Control_t>
                        {
                            Field = $"C{i - 1}",
                            Operator = Operator_t.Equal,
                            Value = "armed",
                        },
                    }
                );
            }
            panel.Controls.Add(control);
        }

        var model = new EditViewModel(strategy);

        model.StrategyErrors.Should().BeEmpty();
        model.HasErrors.Should().BeFalse();
        foreach (var control in model.Controls)
        {
            control.Value.Should().Be("armed");
        }
    }
}
