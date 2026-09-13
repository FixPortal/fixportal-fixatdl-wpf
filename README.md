![FixAtdl WPF: editable strategy forms from FixPortal core models](https://raw.githubusercontent.com/FixPortal/fixportal-fixatdl-wpf/main/docs/images/fixatdl-wpf-hero.png)

# FixPortal.FixAtdl.Wpf

A .NET 10 WPF library that renders editable FIXatdl strategy forms from
`FixPortal.FixAtdl` models. The library supplies the form; the host owns its
window, application lifecycle and order submission.

The adapter supports all 15 FIXatdl control types, nested panel layouts,
single and multiple list selections, editable dropdowns, state rules and
parameter/strategy validation. Templates load automatically.

## Use in a WPF application

Target `net10.0-windows` with `UseWPF` enabled. Install
[`FixPortal.FixAtdl.Wpf`](https://www.nuget.org/packages/FixPortal.FixAtdl.Wpf/)
from NuGet.org:

```sh
dotnet add package FixPortal.FixAtdl.Wpf
```

The package brings in
[`FixPortal.FixAtdl.Wpf.Core`](https://www.nuget.org/packages/FixPortal.FixAtdl.Wpf.Core/),
the platform-independent view-model, state-rule and validation layer. It in
turn uses the headless
[`FixPortal.FixAtdl`](https://www.nuget.org/packages/FixPortal.FixAtdl/) core.

Related projects:

- [`FixPortal.FixAtdl`](https://github.com/FixPortal/fixportal-fixatdl) — the
  .NET parser, model, validator and FIX-tag emitter.
- [`FixPortal.FixAtdl.Wpf.Core`](https://github.com/FixPortal/fixportal-fixatdl-wpf/tree/main/src/FixPortal.FixAtdl.Wpf.Core)
  — the platform-independent layer published with this repository.
- [`@fix-portal/fixatdl-react`](https://www.npmjs.com/package/@fix-portal/fixatdl-react)
  ([repository](https://github.com/FixPortal/fixportal-fixatdl-react)) — a
  separate React adapter; it is not a dependency of this WPF package.

See [CONTRIBUTING.md](CONTRIBUTING.md) for local builds and the private feed
used only by development tooling.

Register the adapter with your host's service collection:

```csharp
services.AddFixAtdlWpf();
```

After parsing a strategy with `FixPortal.FixAtdl`, initialize its controls
once using the core value provider. For a new order with no existing FIX values:

```csharp
strategy.LoadInitialControlValues(FixFieldValueProvider.Empty);
var (view, editor) = AtdlPanel.Create(strategy, serviceProvider);
contentControl.Content = view;
```

Use `FixPortal.FixAtdl.Wpf` and `FixPortal.FixAtdl.Fix` namespaces.
Create and use the panel on the WPF dispatcher thread. An editor owns mutable
strategy state: use a separate strategy instance for each open editor.
For order amendments, load the existing order through the core before creating
the panel; the adapter preserves loaded control values.

Use `AtdlPanel.Create(strategy, serviceProvider, isAmendment: true)` or
`new EditViewModel(strategy, isAmendment: true)` to enforce `mutableOnCxlRpl="false"`.
Immutable values stay disabled and reject direct assignments and state-rule
changes. A radio group with an immutable selected member is also locked.

```csharp
if (!editor.HasErrors)
{
    IReadOnlyDictionary<int, string> values = editor.ReadBackFixValues();
    // Pass these validated FIX tag/value pairs to the host's order builder.
}
```

`ControlViewModel` implements `INotifyDataErrorInfo`. Parameter errors are
available on each control, and `editor.StrategyErrors` contains strategy-level
validation failures. `ReadBackFixValues()` throws if the editor has errors.
Values use the parameter's FIX wire conversion, including enum mappings,
boolean wire values and UTC clock values.

Clock controls use their core `Clock_t.Clock` and `LocalMktTz` settings.
The time picker passes a time-only value to that boundary; it never chooses a
date from the desktop's local clock. Configure the core clock before loading
initial values when reproducible time behavior is needed.

## Conformance scope

State-rule tests share `Fixtures/Conformance/state-transitions.json` with the
headless core and React adapter. They cover initial false enabled/visible
inversion, `{NULL}` clear/restore, ordinary value transitions, cascades and
cycle rejection against FIXatdl 1.1 state-rule conventions i–v.

Radio controls sharing a parameter emit one FIX tag from the selected member;
complementary mappings and null unchecked mappings are covered. Spinner buttons use the declared
increment; Tick/LotSize policies fall back to that increment because the adapter
has no instrument-data provider. Parameter limits block submission of invalid
values; the buttons do not clamp them.

Sliders with ListItems render enumerated choices; sliders without ListItems
render a continuous numeric range using parameter bounds and the declared
increment. Percentage bounds are displayed in whole-percent units. Unset values
stay unset during rendering and can be cleared explicitly. Without declared
bounds, the numeric slider uses 0–100 (or the parameter type's minimum).
If only a negative maximum or a minimum above 100 is declared, the missing
opposite bound leaves a range of 100 display units, capped at decimal limits.
The clock editor exposes hours and minutes; editing sets seconds to zero.
Full FIXatdl conformance is not claimed: message construction (including tag 957
groups), instrument data and cancel/replace message policy belong to the host.

## Build and release

Windows and the .NET 10 SDK are required for the WPF project and UI tests.
The core project and its tests run without WPF. CI checks formatting, builds,
tests both projects on Windows and packs both libraries.

Tags `vMAJOR.MINOR.PATCH` on commits reachable from `main` publish both packages
to NuGet.org (and GitHub Packages) after CI succeeds. Creating a tag is a
release action; ordinary branch and pull-request builds only produce artifacts.

The renderer creates and parses XAML. See [SECURITY.md](SECURITY.md) for its
trust boundaries and vulnerability reporting.

## Attribution

The control and layout code derives from Steve Wilkinson's MIT-licensed
Atdl4net WPF implementation (2010–2011). FixPortal modifications are
Apache-2.0 licensed. [NOTICE](NOTICE) preserves upstream attribution and
the MIT terms; [LICENSE](LICENSE) contains the Apache-2.0 terms.
