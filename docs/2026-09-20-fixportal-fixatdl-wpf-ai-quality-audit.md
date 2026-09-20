---
project: fixportal-fixatdl-wpf
review-type: ai-quality-audit
run-id: 20260920T144852Z
date: 2026-09-20
commit: 9e55a6c060e8c2bf4ae9ff0cd26e767123ed21b4
corpus: ai-code-quality v5
corpus-age-days: 1.8
seats-reporting: 5 of 5
findings: 23
unclaimed-findings: 8
anchors-checked: 23
anchors-marked: 7
disposition: reported
tags:
  - audit/ai-quality
  - project/fixportal-fixatdl-wpf
---

# fixportal-fixatdl-wpf - AI quality audit

5 models audited `fixportal-fixatdl-wpf` at commit `9e55a6c060e8` against 15 specific published criticisms of AI-written code, taken from corpus `ai-code-quality` v5. For each criticism the question was simply: does this repository do the thing the criticism describes?

## Verdict

**Largely not slop.** Of the 15 published criticisms this repository could be checked against, 1 applies. The rest came back clean or contested.

7 clean, 1 exhibited, 7 contested.

What applies, and what it is:

- **C04** (asserted) - AI assistance increases duplicated code and reduces refactoring and code reuse.
- **C05** (contested) - AI-generated C# ignores nullable reference type annotations: it omits null checks and assigns possibly-null results to non-nullable targets.
- **C06** (contested) - AI-generated .NET code neglects disposal of resources, reaches for generic exception types, and applies null-checking inconsistently.
- **C09** (contested) - This repository's own tests are too few or too weak to falsify the code they cover: they exercise the happy path, assert loosely, or draw inputs from a pool that cannot reach the failing case.
- **C10** (contested) - AI-generated code can appear to work while breaking core functionality, revealed only by thorough testing.
- **C13** (contested) - LLM-generated code carries recurring bug patterns that differ from human-written defects.
  - Counterpoint (**C26**): Comparing like for like, human-written code showed a greater variety of security problems than GPT-4 code; the generated code differed by containing more severe outliers, not more defects.
- **C15** (contested) - Coverage and mutation adequacy do not catch the faults LLM-generated code actually contains, because the test oracles fail to capture the faulty behaviour.
- **C16** (contested) - LLM-generated unit tests carry test smells -- design flaws that undermine readability and maintainability -- beyond what compilability or coverage reveals.

The panel raised 23 findings in total: 15 matching a published criticism, and 8 matching none of them. The second group is the more interesting one -- the corpus was written about AI-generated code in general, not about this repository. 

## What was found

23 findings, each anchored to a file and line. A finding marked "failed - demonstrated" is covered by a probe that ran and showed the defect. A seat emits ONE probe and names the CLAIM it demonstrates, not a single finding -- so where that seat raised several findings under the same claim, the cell marks each of them, and the probe count in Evidence quality is the number of probes rather than the number of rows. "not probed" means this seat's probe addressed a different claim; one marked "passed - unsubstantiated" is the seat's assertion with no executable evidence behind it. "failed - not attributable" means the probe command exited non-zero in a clean room that could not run this repository's tests in the first place, so the exit says nothing about the finding. The Role column separates an independent issue from a second reviewer's account of the same one: several seats landing on one file and line is CORROBORATION, and a flat table renders it as volume instead. The strongest account of a location is marked independent and names the seats that corroborated it; the rest are marked supporting and point at it. Nothing is merged -- each seat's own wording is evidence about that seat, and folding them would discard the independence a cross-vendor panel exists to buy.


By declared severity: 1 high, 7 medium, 15 low. 23 findings sit at 22 distinct file and line positions, of which 1 was reached independently by more than one seat -- 1 of the row below is that seat's separate account of a location already listed, marked supporting in the Role column rather than merged away.

### Matching a published criticism

| Severity | Finding | Seat | Role | Claim | Anchor | Probe | Summary |
|---|---|---|---|---|---|---|---|
| medium | F1 | C | independent | [C09](#c09) | `src/FixPortal.FixAtdl.Wpf.Core/ViewModels/EditViewModel.cs:293` OK | failed - demonstrated | Two {NULL} state rules on one control: deactivating both restores the second rule's snapshot of the already-cleared value, silently discarding the trader's entry. |
| medium | F4 | R | independent | [C13](#c13) | `src/FixPortal.FixAtdl.Wpf/Controls/TimePicker.xaml:20` OK | failed - not attributable | TimePicker RepeatButton chrome uses WinForms System.Drawing.SystemColors as a DynamicResource key; sibling spinners correctly use WPF ControlDarkBrushKey. |
| medium | F1 | X | independent | [C10](#c10) | `src/FixPortal.FixAtdl.Wpf.Core/ViewModels/ControlViewModel.cs:189` RECOVERED (quote at line 192) | failed - not attributable | A state rule writes the control before validation, so an unexpected parameter-write exception can leave HasErrors false and permit stale FIX read-back. Track the state-rule write within the same torn-write protection as direct edits. |
| medium | F1 | R | independent | [C04](#c04) | `src/FixPortal.FixAtdl.Wpf/Rendering/DefaultRendering/CheckBoxListRenderer.cs:31` OK | not probed | List renderers are copy-pasted clones of the same labelled-control XAML emit, including the DataContext format string. |
| medium | F3 | R | independent, corroborated by C | [C05](#c05) | `src/FixPortal.FixAtdl.Wpf.Core/ViewModels/ListControlViewModel.cs:42` OK | not probed | ListControlViewModel casts GetCurrentValue() to EnumState with no null guard; SelectedValue/Text/item refresh all go through it. |
| medium | F6 | R | independent | [C15](#c15) | `tests/FixPortal.FixAtdl.Wpf.Tests.UI/TimePickerWrapTests.cs:13` RECOVERED (quote at line 16) | not probed | TimePicker wrap tests exercise the control and never assert RepeatButton brushes, so the System.Drawing resource-key fault cannot fail them. |
| low | F1 | K | independent | [C04](#c04) | `src/FixPortal.FixAtdl.Wpf/Controls/SingleSpinner.xaml.cs:69` OK | failed - not attributable | SingleSpinner and DoubleSpinner code-behind event handlers are near-verbatim duplicates (modulo Inner/Outer naming), and the same is true of the per-file Descendants/Layout helpers copied into six UI test files. |
| low | F5 | R | independent | [C13](#c13) | `src/FixPortal.FixAtdl.Wpf/Controls/NumericSpinnerControlBase.cs:19` RECOVERED (quote at line 18) | failed - not attributable | NumericSpinnerControlBase.ValueProperty still carries the DoubleSpinner InnerIncrement doc comment above the real summary. |
| low | F3 | C | independent | [C06](#c06) | `src/FixPortal.FixAtdl.Wpf/Rendering/WpfControlRenderer.cs:204` OK | not probed | Null-argument guard applied on exactly one of sixteen IControlVisitor.Visit overloads in WpfControlRenderer. |
| low | F4 | C | independent | [C04](#c04) | `src/FixPortal.FixAtdl.Wpf/Rendering/DefaultRendering/DropDownListRenderer.cs:41` OK | not probed | Four list renderers carry the same three-line rationale comment verbatim and near-identical attribute-emission bodies. |
| low | F7 | C | independent | [C16](#c16) | `tests/FixPortal.FixAtdl.Wpf.Core.Tests/ViewModels/RefreshRulesIdempotencyTests.cs:71` OK | not probed | RefreshRulesIdempotencyTests drives the system under test through private-method reflection, coupling the test to a name the compiler will not check. |
| low | F8 | C | independent | [C06](#c06) | `tests/FixPortal.FixAtdl.Wpf.Tests.UI/AtdlPanelTests.cs:642` OK | not probed | Three of 21 BuildServiceProvider sites in AtdlPanelTests.cs omit the `using` the other eighteen use, leaking the provider for the life of the test run. |
| low | F2 | R | independent | [C05](#c05) | `src/FixPortal.FixAtdl.Wpf/Rendering/StrategyPanelRenderer.cs:44` OK | not probed | Possibly-null StrategyPanel is assigned to a non-nullable local, then null-checked. |
| low | F7 | R | independent | [C16](#c16) | `tests/FixPortal.FixAtdl.Wpf.Tests.UI/AtdlPanelTests.cs:583` RECOVERED (quote at line 582) | not probed | The same Descendants tree-walk helper is duplicated across UI test classes instead of one shared fixture. |
| low | F2 | C | supporting F3/R | [C05](#c05) | `src/FixPortal.FixAtdl.Wpf.Core/ViewModels/ListControlViewModel.cs:42` OK | not probed | ListControlViewModel.CurrentState casts a possibly-null control value to non-nullable EnumState; an empty Slider_t through the public constructor yields NullReferenceException on every read. |

### Not named by any criticism in the corpus

The corpus was written about AI-generated code in general, not about this repository. These findings map to none of its claims, which is where the corpus stops bounding the audit.

| Severity | Finding | Seat | Role | Claim | Anchor | Probe | Summary |
|---|---|---|---|---|---|---|---|
| high | F1 | G | independent | - | `src/FixPortal.FixAtdl.Wpf.Core/ViewModels/EditViewModel.cs:341` RECOVERED (quote at line 44) | failed - not attributable | EditViewModel constructor aggressively rejects valid strategies with duplicate FIX tags. |
| medium | F8 | R | independent | - | `src/FixPortal.FixAtdl.Wpf/Controls/TimePicker.xaml:39` RECOVERED (quote at line 40) | not probed | TimePicker column 0 is an unused star-width column; hours sit in column 1 inside a renderer-fixed 75px width, so chrome is squeezed by empty space. |
| low | F5 | C | independent | - | `src/FixPortal.FixAtdl.Wpf/Rendering/StrategyPanelRenderer.cs:101` OK | not probed | StrategyPanelRenderer threads a `depth` counter by ref through the whole panel recursion and never reads it; nested venue panels are rendered with no depth bound. |
| low | F6 | C | independent | - | `src/FixPortal.FixAtdl.Wpf.Core/ViewModels/ControlViewModel.cs:44` OK | not probed | ControlViewModel.WireValue is dead code: a HasErrors/HasTornWrite safety gate with zero callers in src, tests or XAML, so it reads as protection that is not wired to anything. |
| low | F9 | C | independent | - | `src/FixPortal.FixAtdl.Wpf.Core/ViewModels/ListItemViewModel.cs:29` OK | not probed | ListItemViewModel.IsRequiredParameter has no consumer in any renderer, resource dictionary or test; the required-field cue is driven off the label instead. |
| low | F2 | K | independent | - | `src/FixPortal.FixAtdl.Wpf.Core/ViewModels/ControlViewModel.cs:71` RECOVERED (quote at line 44) | not probed | ControlViewModel.WireValue is dead code: it is internal, is never read anywhere in the change (EditViewModel.ReadBackFixValues reads IParameter.WireValue directly), so the compiler cannot flag it and it exists only as surface area. |
| low | F3 | K | independent | - | `src/FixPortal.FixAtdl.Wpf.Core/ViewModels/ControlViewModel.cs:1` OK | not probed | Every source file header says 'Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE', but the change adds no NOTICE file and neither csproj packs one, so the referenced attribution does not ship with the IsPackable=true libraries. |
| low | F9 | R | independent | - | `src/FixPortal.FixAtdl.Wpf.Core/ViewModels/ControlViewModel.cs:67` OK | not probed | MakeReadOnly mutates IsReadOnly without notifying Enabled or IsReadOnly; currently only called during EditViewModel construction, before DataContext is applied. |

## Coverage: every criticism, and what each seat said

One row per published criticism. The verdict column means:

- **confirmed** - a seat found it AND a probe demonstrated it
- **asserted** - a seat found it, with no probe evidence
- **contested** - the seats disagreed; the disagreement is preserved, never averaged
- **clean** - every seat checked it and none found it
- **clean (partial)** - everyone who checked said no, but not everyone checked
- **not assessed** - no seat examined it. This is not a weaker "clean".

The **Looked** column counts how many reporting seats actually examined that criticism. A verdict backed by one seat is weaker evidence than the same verdict backed by all of them, and a bare matrix hides the difference.

| # | Criticism | X | C | K | G | R | Looked | Verdict |
|---|---|---|---|---|---|---|---|---|
| C02 | AI-generated code reproduces exploitable defects because it was trained on unvetted, buggy code. | not assessed | clean | clean | not assessed | clean | 3 of 5 | clean (partial) |
| C04 | AI assistance increases duplicated code and reduces refactoring and code reuse. | not assessed | exhibits | exhibits | not assessed | exhibits | 3 of 5 | asserted |
| C05 | AI-generated C# ignores nullable reference type annotations: it omits null checks and assigns possibly-null results to non-nullable targets. | not assessed | exhibits | clean | not assessed | exhibits | 3 of 5 | contested |
| C06 | AI-generated .NET code neglects disposal of resources, reaches for generic exception types, and applies null-checking inconsistently. | clean | exhibits | clean | not assessed | clean | 4 of 5 | contested |
| C07 | AI-generated tests assert general outcomes rather than specific values, and omit coverage for new public methods. | not assessed | clean | clean | not assessed | clean | 3 of 5 | clean (partial) |
| C08 | AI-generated code references packages that do not exist, creating a supply-chain attack surface. | not assessed | clean | clean | not assessed | clean | 3 of 5 | clean (partial) |
| C09 | This repository's own tests are too few or too weak to falsify the code they cover: they exercise the happy path, assert loosely, or draw inputs from a pool that cannot reach the failing case. | exhibits | exhibits | clean | not assessed | clean | 4 of 5 | contested |
| C10 | AI-generated code can appear to work while breaking core functionality, revealed only by thorough testing. | exhibits | exhibits | clean | not assessed | clean | 4 of 5 | contested |
| C11 | AI-generated code solves the immediate task but misses long-term maintainability and architectural fit. | clean | clean | clean | not assessed | clean | 4 of 5 | clean (partial) |
| C12 | AI-generated code introduces security flaws at a high rate across major languages, C# included. | not assessed | clean | clean | not assessed | clean | 3 of 5 | clean (partial) |
| C13 | LLM-generated code carries recurring bug patterns that differ from human-written defects. | not assessed | exhibits | clean | not assessed | exhibits | 3 of 5 | contested |
| C14 | Generated tests run and pass while asserting weakly: they are executable without meaningfully constraining behaviour, so coverage overstates what they verify. | not assessed | clean | clean | not assessed | clean | 3 of 5 | clean (partial) |
| C15 | Coverage and mutation adequacy do not catch the faults LLM-generated code actually contains, because the test oracles fail to capture the faulty behaviour. | not assessed | not assessed | clean | not assessed | exhibits | 2 of 5 | contested |
| C16 | LLM-generated unit tests carry test smells -- design flaws that undermine readability and maintainability -- beyond what compilability or coverage reveals. | not assessed | exhibits | clean | not assessed | exhibits | 3 of 5 | contested |
| C17 | Benchmarks reporting only functional correctness hide the trade-off against maintainability, efficiency and style -- the qualities that decide whether .NET code survives contact with a team. | not assessed | clean | clean | not assessed | not assessed | 2 of 5 | clean (partial) |

## Practice: disciplines the corpus argues for

These are not allegations against this repository. The corpus carries them because they say why a control exists, and the question is whether this repository follows the discipline - so they are counted nowhere in the verdict above.

- **follows** - the repository demonstrably does this
- **does not follow** - it does not, which is not by itself a defect
- **not assessed** - no seat could tell from source alone

| # | Practice | X | C | K | G | R | Looked | Verdict |
|---|---|---|---|---|---|---|---|---|
| C20 | Red-green TDD is the working discipline for agent-written code: the agent is given the test command first and made to drive the change from a failing test. | not assessed | not assessed | not assessed | not assessed | does not follow | 1 of 5 | does not follow (partial) |
| C22 | Static analysis and test feedback fed back into generation measurably improves the code produced, which is the argument for treating analyzers as build-blocking rather than advisory. | not assessed | follows | not assessed | not assessed | not assessed | 1 of 5 | follows |

### What the matrix does not mean

Every seat audited against the same corpus. That is deliberate - it makes divergence attributable to the model rather than to what each one happened to read - and it means agreement across seats is a **control, not reassurance**. A shared frame produces shared conclusions, so a row of "clean" is evidence about the panel before it is evidence about the code.

## Evidence quality

probe baseline: `dotnet restore "FixPortal.FixAtdl.Wpf.slnx"; dotnet build "FixPortal.FixAtdl.Wpf.slnx" -c Release --no-restore; dotnet test --solution "FixPortal.FixAtdl.Wpf.slnx" -c Release --no-build --timeout 5m` ran green in the clean room, so a probe failure is attributable to its finding

probes: 5 emitted, 5 ran, 1 substantiated, 0 timed out

anchor-validation: 23 checked, 7 marked (retained, never dropped)

Anchors are checked mechanically: the file must exist at that path, the line must be in range, and any quoted code must match at that line. A finding whose anchor fails is marked in the tables above and kept - deleting it would discard the evidence that settled it.

## How this was produced

### Panel

- **X** (openai) - resolved to `gpt-6-astra` at dispatch
- **C** (anthropic) - resolved to `claude-opus-5` at dispatch
- **K** (moonshot) - resolved to `kimi-code/kimi-for-coding` at dispatch
- **G** (google) - resolved to `gemini-3.1-pro-high` at dispatch
- **R** (xai) - resolved to `grok-4.6` at dispatch

### Clean room

The panel audited an ephemeral git worktree at this commit, not this checkout. Nothing needed deleting from it: this repository carries no previous audit output and no findings ledger, so there was nothing a seat could have read and restated as fresh analysis.

## Implementation follow-up — 2026-09-20

The maturity plan was implemented through rebase-merged pull requests:

- [FixPortal/fixportal-codestyle#65](https://github.com/FixPortal/fixportal-codestyle/pull/65)
  and [#66](https://github.com/FixPortal/fixportal-codestyle/pull/66) — made the
  portable `FixPortal.CodeStyle` package public through NuGet.org OIDC, retained
  the private `FixPortal.CodeStyle.ArchRules` overlay, and corrected the release
  action pin.
- [FixPortal/fixportal-fixatdl-wpf#33](https://github.com/FixPortal/fixportal-fixatdl-wpf/pull/33)
  — added contribution templates, the OSS maturity plan, and corrected the
  documented `FixPortal.FixAtdl` version.
- [FixPortal/fixportal-fixatdl-wpf#34](https://github.com/FixPortal/fixportal-fixatdl-wpf/pull/34)
  — removed the private-feed restore requirement. Restore now uses NuGet.org
  without FixPortal credentials.
- [FixPortal/fixportal-fixatdl-wpf#36](https://github.com/FixPortal/fixportal-fixatdl-wpf/pull/36)
  — updated the architecture README image/text and committed this report.

The WPF migration was validated without `GITHUB_PACKAGES_TOKEN`: CSharpier,
restore, Release build with 0 warnings/errors, 197 tests, vulnerability scan
with no vulnerable packages, package creation, and actionlint all passed.
The final documentation follow-up also passed the full repository gate.

GitHub Secret Scanning and Push Protection were enabled already. Dependabot
security updates are enabled, and `main` is protected with strict `CI Gate` and
`Review policy intact` checks, linear history, conversation resolution, admin
enforcement, and no force-push/deletion allowance. The required approval count
is explicitly zero, matching the canonical solo-maintainer `scaffold-repo`
ruleset. The plan is fully actioned as of the merge of PR #36.

## Sources

Every criticism and practice above is quoted from published work, not from the panel's own opinion. These are the sources the claims in this report rest on. The corpus holds 74 accepted sources in total; the other 53 back no claim used here and are listed for provenance in the run record.

#### C02
AI-generated code reproduces exploitable defects because it was trained on unvetted, buggy code.
- Asleep at the Keyboard? Assessing the Security of GitHub Copilot's Code Contributions <https://arxiv.org/abs/2108.09293>
- A Survey of Bugs in AI-Generated Code <https://arxiv.org/abs/2512.05239>

#### C04
AI assistance increases duplicated code and reduces refactoring and code reuse.
- AI Copilot Code Quality: 2025 Data Suggests Growth in Code Clones <https://www.gitclear.com/ai_assistant_code_quality_2025_research>

#### C05
AI-generated C# ignores nullable reference type annotations: it omits null checks and assigns possibly-null results to non-nullable targets.
- GitHub Copilot unaware of nullable types in C#? <https://github.com/orgs/community/discussions/120409>

#### C06
AI-generated .NET code neglects disposal of resources, reaches for generic exception types, and applies null-checking inconsistently.
- Reviewing AI-Generated Code in .NET <https://devblogs.microsoft.com/dotnet/developer-and-ai-code-reviewer-reviewing-ai-generated-code-in-dotnet/>

#### C07
AI-generated tests assert general outcomes rather than specific values, and omit coverage for new public methods.
- Reviewing AI-Generated Code in .NET <https://devblogs.microsoft.com/dotnet/developer-and-ai-code-reviewer-reviewing-ai-generated-code-in-dotnet/>

#### C08
AI-generated code references packages that do not exist, creating a supply-chain attack surface.
- We Have a Package for You! A Comprehensive Analysis of Package Hallucinations by Code Generating LLMs <https://arxiv.org/abs/2406.10279>

#### C09
This repository's own tests are too few or too weak to falsify the code they cover: they exercise the happy path, assert loosely, or draw inputs from a pool that cannot reach the failing case.
- Is Your Code Generated by ChatGPT Really Correct? Rigorous Evaluation of LLMs for Code Generation <https://arxiv.org/abs/2305.01210>

#### C10
AI-generated code can appear to work while breaking core functionality, revealed only by thorough testing.
- Can chatbots craft correct code? <https://blog.trailofbits.com/2025/12/19/can-chatbots-craft-correct-code/>

#### C11
AI-generated code solves the immediate task but misses long-term maintainability and architectural fit.
- Reviewing AI-Generated Code in .NET <https://devblogs.microsoft.com/dotnet/developer-and-ai-code-reviewer-reviewing-ai-generated-code-in-dotnet/>

#### C12
AI-generated code introduces security flaws at a high rate across major languages, C# included.
- Assessing the Quality and Security of AI-Generated Code: A Quantitative Analysis <https://arxiv.org/abs/2508.14727>
- Security Weaknesses of Copilot-Generated Code in GitHub Projects <https://arxiv.org/abs/2310.02059>

#### C13
LLM-generated code carries recurring bug patterns that differ from human-written defects.
- Bugs in Large Language Models Generated Code: An Empirical Study <https://arxiv.org/abs/2403.08937>

#### C14
Generated tests run and pass while asserting weakly: they are executable without meaningfully constraining behaviour, so coverage overstates what they verify.
- VibeCheck: Assessing the Quality of LLM-Generated Unit Tests <https://arxiv.org/abs/2609.05978>

#### C15
Coverage and mutation adequacy do not catch the faults LLM-generated code actually contains, because the test oracles fail to capture the faulty behaviour.
- How effective are traditional test criteria at detecting bugs in LLM-generated code? <https://arxiv.org/abs/2609.09315>

#### C16
LLM-generated unit tests carry test smells -- design flaws that undermine readability and maintainability -- beyond what compilability or coverage reveals.
- On the Diffusion of Test Smells in LLM-Generated Unit Tests <https://arxiv.org/abs/2410.10628>

#### C17
Benchmarks reporting only functional correctness hide the trade-off against maintainability, efficiency and style -- the qualities that decide whether .NET code survives contact with a team.
- Benchmarking the Titans: LLM Code Generation Quality in the .NET Ecosystem <https://arxiv.org/abs/2608.22529>

#### C20
Red-green TDD is the working discipline for agent-written code: the agent is given the test command first and made to drive the change from a failing test.
- Engineering practices that make coding agents work (Simon Willison) <https://simonwillison.net/2026/Mar/14/pragmatic-summit/>

#### C22
Static analysis and test feedback fed back into generation measurably improves the code produced, which is the argument for treating analyzers as build-blocking rather than advisory.
- Helping LLMs Improve Code Generation Using Feedback from Testing and Static Analysis <https://arxiv.org/abs/2412.14841>

#### C23
Measured across public repositories rather than purpose-generated samples, the large majority of AI-generated code carries no identifiable CWE-mapped vulnerability at all.
- Security Vulnerabilities in AI-Generated Code: 7,703 public GitHub files (87.9% carry no CWE) <https://arxiv.org/abs/2510.26103>

#### C24
The studies raising the alarm on AI code security largely analysed code generated for the study itself, which leaves their realism open to question.
- WildCode Revisited: prior alarm studies used purpose-generated code <https://arxiv.org/abs/2512.04259>

#### C25
A controlled study of subsequent evolution found no significant difference in completion time or code quality between AI-co-developed and human-written code.
- Echoes of AI: Downstream Effects of AI Assistants on Software Maintainability <https://arxiv.org/abs/2507.00788>

#### C26
Comparing like for like, human-written code showed a greater variety of security problems than GPT-4 code; the generated code differed by containing more severe outliers, not more defects.
- Comparing Human and LLM Generated Code: The Jury is Still Out! <https://arxiv.org/abs/2501.16857>


Each claim carries a verbatim quote from its source in the corpus manifest, and every source is pinned by sha256 against the bytes that were scraped, so a claim can be checked against what was actually published rather than against a paraphrase of it.
