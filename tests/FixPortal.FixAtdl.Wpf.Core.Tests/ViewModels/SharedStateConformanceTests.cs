using System.Text.Json;
using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;

namespace FixPortal.FixAtdl.Wpf.Core.Tests.ViewModels;

public class SharedStateConformanceTests
{
    public static IEnumerable<object[]> Scenarios()
    {
        using var document = JsonDocument.Parse(
            File.ReadAllText(
                Path.Combine(AppContext.BaseDirectory, "Fixtures", "Conformance", "state-transitions.json")
            )
        );
        return document
            .RootElement.EnumerateArray()
            .Select(scenario => new object[] { scenario.GetProperty("name").GetString()!, scenario.GetRawText() })
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(Scenarios))]
    public void StateTransitions_MatchSharedSpecificationCases(string name, string json)
    {
        using var document = JsonDocument.Parse(json);
        var scenario = document.RootElement;
        var strategy = new Strategy_t();
        var panel = new StrategyPanel_t(strategy);
        strategy.StrategyLayout = new StrategyLayout_t { StrategyPanel = panel };
        foreach (var definition in scenario.GetProperty("controls").EnumerateArray())
        {
            panel.Controls.Add(CreateControl(definition));
        }
        var model = new EditViewModel(strategy);
        var controls = model.Controls.ToDictionary(control => control.Id);
        foreach (var step in scenario.GetProperty("steps").EnumerateArray())
        {
            if (step.TryGetProperty("set", out var edit))
            {
                controls[edit.GetProperty("id").GetString()!].Value = edit.GetProperty("value").GetString();
            }
            VerifyExpected(step.GetProperty("expected"), model, controls, name);
        }
    }

    private static TextField_t CreateControl(JsonElement definition)
    {
        definition.GetProperty("type").GetString().Should().Be("TextField_t");
        var control = new TextField_t(definition.GetProperty("id").GetString()!);
        control.SetValue(definition.GetProperty("initValue").GetString()!);
        foreach (var rule in definition.GetProperty("stateRules").EnumerateArray())
        {
            var expression = rule.GetProperty("expression");
            expression.GetProperty("kind").GetString().Should().Be("compare");
            var stateRule = new StateRule_t
            {
                Edit = new Edit_t<Control_t>
                {
                    Field = expression.GetProperty("field").GetString()!,
                    Operator = expression.GetProperty("operator").GetString() switch
                    {
                        "==" => Operator_t.Equal,
                        "exists" => Operator_t.Exist,
                        var value => throw new InvalidOperationException($"Unsupported operator: {value}"),
                    },
                    Value = expression.GetProperty("value").GetString()!,
                },
            };
            switch (rule.GetProperty("effect").GetString())
            {
                case "enabled":
                    stateRule.Enabled = rule.GetProperty("targetValue").GetBoolean();
                    break;
                case "visible":
                    stateRule.Visible = rule.GetProperty("targetValue").GetBoolean();
                    break;
                case "value":
                    stateRule.Value = rule.GetProperty("targetStringValue").GetString()!;
                    break;
                default:
                    throw new InvalidOperationException("Unsupported effect");
            }
            control.StateRules.Add(stateRule);
        }
        return control;
    }

    private static void VerifyExpected(
        JsonElement expected,
        EditViewModel model,
        Dictionary<string, ControlViewModel> controls,
        string name
    )
    {
        if (expected.TryGetProperty("values", out var values))
        {
            foreach (var value in values.EnumerateObject())
            {
                controls[value.Name].Value.Should().Be(value.Value.GetString(), name);
            }
        }
        if (expected.TryGetProperty("states", out var states))
        {
            foreach (var state in states.EnumerateObject())
            {
                if (state.Value.TryGetProperty("enabled", out var enabled))
                {
                    controls[state.Name].Enabled.Should().Be(enabled.GetBoolean(), name);
                }
                if (state.Value.TryGetProperty("visible", out var visible))
                {
                    controls[state.Name].Visible.Should().Be(visible.GetBoolean(), name);
                }
            }
        }
        model.HasErrors.Should().Be(expected.TryGetProperty("hasErrors", out var errors) && errors.GetBoolean(), name);
        if (model.HasErrors)
        {
            var read = () => model.ReadBackFixValues();
            read.Should().Throw<InvalidOperationException>();
        }
    }
}
