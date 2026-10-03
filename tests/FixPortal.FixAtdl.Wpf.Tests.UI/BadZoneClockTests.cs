using System.ComponentModel.DataAnnotations;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// Audit H5: a venue typo in LocalMktTz must surface as a control validation error, not a host crash.
/// The core's Clock_t throws InvalidFieldValueException (a FixAtdlException-family type) for
/// unrecognised IANA zones, and ControlViewModel.ValidateAgainstAtdlConstraints converts it through
/// the catch filter at ControlViewModel.cs:176-183 - a filter arm with zero coverage in this repo.
/// The clock is created with a valid zone and a loaded value; the bad zone id is introduced only
/// AFTER AtdlPanel.Create by mutating the model, and the edit is driven through the
/// ControlViewModel.Value setter so the throw lands inside the callback's try block (the
/// GetCurrentValue comparison at ControlViewModel.cs:165 reaches Clock_t.GetCurrentValue, which
/// validates the zone once a value is loaded). The clock must hold a value: Clock_t.GetCurrentValue
/// short-circuits null values before the zone lookup, so a value-less bad-zone clock has no edit
/// path that consults the zone. Setting the bad zone before creation would instead throw in the
/// ControlViewModel constructor (Snapshot(control.GetCurrentValue()) runs unprotected), crashing
/// before the validation callback is ever reached.
/// </summary>
public class BadZoneClockTests
{
    [Fact]
    public void UnrecognisedLocalMktTzSurfacesAsValidationError()
    {
        StaTestHarness.Run(() =>
        {
            var strategy = new FixPortal.FixAtdl.Model.Elements.Strategy_t();
            var panel = new FixPortal.FixAtdl.Model.Elements.StrategyPanel_t(strategy);
            strategy.StrategyLayout = new FixPortal.FixAtdl.Model.Elements.StrategyLayout_t { StrategyPanel = panel };
            var control = new FixPortal.FixAtdl.Model.Controls.Clock_t("Clock")
            {
                ParameterRef = "Clock",
                LocalMktTz = "America/New_York",
            };
            control.SetValue(new DateTime(2026, 1, 1, 10, 30, 0, DateTimeKind.Utc));
            strategy.Parameters.Add(
                new FixPortal.FixAtdl.Model.Elements.Parameter_t<FixPortal.FixAtdl.Model.Types.UTCTimestamp_t>("Clock")
                {
                    FixTag = 9003,
                }
            );
            panel.Controls.Add(control);
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (_, model) = AtdlPanel.Create(strategy, services);
            model.HasErrors.Should().BeFalse("a loaded clock with a valid zone starts clean");

            // The venue typo lands only now, after panel creation, so the InvalidFieldValueException
            // fires inside the validation callback's try block instead of the view-model constructor.
            control.LocalMktTz = "Mars/Olympus_Mons";
            var edit = () => model.Controls[0].Value = new DateTime(2026, 6, 2, 9, 45, 0, DateTimeKind.Utc);

            edit.Should().NotThrow();
            model.HasErrors.Should().BeTrue();
            model
                .Controls[0]
                .GetErrors()
                .Cast<object>()
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<ValidationResult>()
                .Which.ErrorMessage.Should()
                .Contain("localMktTz 'Mars/Olympus_Mons' is not a recognised IANA time zone");
            var read = () => model.ReadBackFixValues();
            read.Should().Throw<InvalidOperationException>();
        });
    }
}
