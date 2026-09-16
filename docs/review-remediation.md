# Whole-library review remediation

> **Historical record — findings as disposed on 2026-09-11.** Kept as
> the rationale behind the first public README, CI, and template work.
> For current host integration see [usage.md](usage.md). The "deferred
> renderer diagnostics" row is stale: HEAD throws `NotSupportedException`
> naming the control type and id.

This pass addresses the final review recorded on 2026-09-11.

| Finding | Disposition |
|---|---|
| C1: incompatible bindings | Renderers use numeric control indexes and the current Value/Items contract. Presentation properties and validation are wired. |
| C2: unreachable list models | EditViewModel constructs ListControlViewModel for every list control. |
| C3: dead sizing infrastructure | Removed WpfComboBoxSizer, unused renderer enums and unused XAML tag registrations. |
| C4: unused pins | Removed Hosting, NodaTime and its JSON serialization pins; no direct consumer exists. |
| S1: markup-extension injection | Literal attributes escape leading braces; XAML names encode IDs; bindings contain generated indexes. |
| S2: malformed or duplicate IDs | Punctuation is safely encoded; empty/duplicate IDs fail before panel parsing. |
| S3: inaccurate security policy | SECURITY.md describes XAML composition, parsing and custom-renderer trust. |
| R1: missing templates | Ported resources, merged them into rendered panels and added default theme discovery. |
| R2: radio grouping | Groups are emitted without legacy compilation guards, scoped per rendered panel and synchronized in the model. |
| R3: ambiguous casts | Parent interfaces use direct casts. |
| R4: invalid unused comparisons | Removed TimeInstant comparison operators. |
| R5: static current date | TimePicker passes time-only values to the core Clock_t clock/zone boundary. |
| R6: slider items mismatch | Slider accepts the current list-item collection and handles zero items without negative dimensions. |
| P1: missing CI | Windows build/test/pack workflow, CI Gate, review-policy guard and canonical checker scripts added. |
| P2: missing README/NOTICE | Added host integration instructions and upstream attribution including MIT terms. |
| P3: packaging | Both projects produce NuGet packages with metadata, README/NOTICE/LICENSE and a tag-gated publish job. |
| P4: missing edit triggers | State-rule transitions, null restoration, bounded cycle detection and strategy validation implemented. |
| P5: list capabilities | Multiple selection and editable free text supported; removed conflicting editable ComboBox selection binding. |
| OSS cleanup | Removed obsolete regions, task/reviewer jargon, Atdl4net namespace constants and the three no-value tests. Standardized test command and JSON newlines. |
| Deferred renderer diagnostics | Existing unknown-renderer lookup remains; all 15 registered control types are asserted. |
| Deferred FIX tag cast | Added checked conversion. |

Additional regressions cover actual spinner text input and culture, preservation
of bindings after repeated edits, invalid-to-valid strategy refresh, clock field
validation and clearing, editable dropdown updates, and preservation of explicitly
cleared amendment values. Invalid state-rule values surface as strategy errors;
they remain errors while active and recover when their condition becomes false.
The public renderer also rejects empty/duplicate IDs before emitting XAML;
the registration test asserts the exact supported control types.

PR review dispositions: the proposed double-quote decoder change was declined
because its sample condition is invalid YAML (confirmed by actionlint). The
shared gate checker's handling of workspace-prefixed script paths is deferred:
this repository uses literal relative paths, all of which are tiered HIGH.
Dropping `/` from the proposed regex lookbehind would also misidentify nested
vendor paths when a same-named root script exists. Broader prefix handling belongs
in the canonical checker with its regression suite.

## Initialization and compatibility

The host must initialize a fresh strategy through the core
`Strategy_t.LoadInitialControlValues` API before panel creation. This preserves
the core's initialization policies and allows an amendment's deliberate false or
null values to survive. A fresh binary control and an explicitly loaded false
value cannot be distinguished through the current core API.

Each editor requires its own mutable Strategy_t. Model collections are fixed
when the editor is constructed. State-rule scans are bounded; a nonconverging
rule graph blocks submission with a strategy error.

The renderer extension API changed: WpfXmlWriter now receives the strategy
control collection; WpfControlRenderer no longer receives a combo-box sizer.
Namespace constants use ControlsNamespace/ControlsNamespaceUri. Invalid
strategies now cause ReadBackFixValues to throw instead of emitting partial data.

## Verification

Local verification includes Release build, both test projects on Windows,
CSharpier, actionlint, workflow hygiene/gate checks and the canonical checkers'
own regression suites. Both NuGet packages were inspected for dependencies,
licence files and absence of the private build-only CodeStyle dependency.

GitHub repository protection settings are separate administrative state; the
repository had no rulesets when this pass was prepared.
