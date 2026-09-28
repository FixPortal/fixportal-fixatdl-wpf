---
project: fixportal-fixatdl-wpf
review-type: ai-quality-audit
run-id: 20260925T213558Z
date: 2026-09-25
commit: 75ba0df17f5c2cee953e8921c0cf9b2221f0fe0f
corpus: ai-code-quality v5
corpus-age-days: 7.1
seats-reporting: 3 of 3
findings: 20
unclaimed-findings: 5
anchors-checked: 20
anchors-marked: 1
disposition: reported
tags:
  - audit/ai-quality
  - project/fixportal-fixatdl-wpf
---

# fixportal-fixatdl-wpf - AI quality audit

3 models audited `fixportal-fixatdl-wpf` at commit `75ba0df17f5c` against 15 specific published criticisms of AI-written code, taken from corpus `ai-code-quality` v5. For each criticism the question was simply: does this repository do the thing the criticism describes?

## Verdict

No overall verdict: 5 of 15 published criticisms were not fully examined (5 partial, 0 unexamined). 3 apply outright and 6 are contested.

1 clean, 3 exhibited, 6 contested, 5 partial.

What applies, and what it is:

- **C04** (asserted) - AI assistance increases duplicated code and reduces refactoring and code reuse.
- **C15** (asserted) - Coverage and mutation adequacy do not catch the faults LLM-generated code actually contains, because the test oracles fail to capture the faulty behaviour.
- **C16** (asserted) - LLM-generated unit tests carry test smells -- design flaws that undermine readability and maintainability -- beyond what compilability or coverage reveals.
- **C06** (contested) - AI-generated .NET code neglects disposal of resources, reaches for generic exception types, and applies null-checking inconsistently.
- **C07** (contested) - AI-generated tests assert general outcomes rather than specific values, and omit coverage for new public methods.
- **C09** (contested) - This repository's own tests are too few or too weak to falsify the code they cover: they exercise the happy path, assert loosely, or draw inputs from a pool that cannot reach the failing case.
- **C10** (contested) - AI-generated code can appear to work while breaking core functionality, revealed only by thorough testing.
- **C11** (contested) - AI-generated code solves the immediate task but misses long-term maintainability and architectural fit.
  - Counterpoint (**C25**): A controlled study of subsequent evolution found no significant difference in completion time or code quality between AI-co-developed and human-written code.
- **C14** (contested) - Generated tests run and pass while asserting weakly: they are executable without meaningfully constraining behaviour, so coverage overstates what they verify.

The panel raised 20 findings in total: 15 matching a published criticism, and 5 matching none of them. The second group is the more interesting one -- the corpus was written about AI-generated code in general, not about this repository. 

## What was found

20 findings, each anchored to a file and line. A finding marked "failed - demonstrated" is covered by a probe that ran and showed the defect. A seat emits ONE probe and names the CLAIM it demonstrates, not a single finding -- so where that seat raised several findings under the same claim, the cell marks each of them, and the probe count in Evidence quality is the number of probes rather than the number of rows. "not probed" means this seat's probe addressed a different claim; one marked "passed - unsubstantiated" is the seat's assertion with no executable evidence behind it. "failed - not attributable" means the probe command exited non-zero in a clean room that could not run this repository's tests in the first place, so the exit says nothing about the finding. The Role column separates an independent issue from a second reviewer's account of the same one: several seats landing on one file and line is CORROBORATION, and a flat table renders it as volume instead. The strongest account of a location is marked independent and names the seats that corroborated it; the rest are marked supporting and point at it. Nothing is merged -- each seat's own wording is evidence about that seat, and folding them would discard the independence a cross-vendor panel exists to buy.


By declared severity: 1 high, 2 medium, 17 low. 20 findings sit at 19 distinct file and line positions, of which 1 was reached independently by more than one seat -- 1 of the row below is that seat's separate account of a location already listed, marked supporting in the Role column rather than merged away.

### Matching a published criticism

| Severity | Finding | Seat | Role | Claim | Anchor | Probe | Summary |
|---|---|---|---|---|---|---|---|
| high | F1 | X | independent | [C10](#c10) | `src/FixPortal.FixAtdl.Wpf.Core/ViewModels/ControlViewModel.cs:191` OK | failed - demonstrated | State rules write the control before validation tracks mutation, so an internal parameter failure leaves HasErrors false and permits stale FIX read-back. Include state-rule writes in the torn-write guard. |
| medium | F2 | X | independent | [C09](#c09) | `tests/FixPortal.FixAtdl.Wpf.Core.Tests/ViewModels/ReviewRegressionTests.cs:127` OK | not probed | The state-rule regression passes because its required Qty is already missing, masking the broken read-back barrier. Populate Qty and assert a clean baseline before triggering the failure. |
| low | F2 | C | independent | [C09](#c09) | `.github/scripts/assert_release_eligibility.py:53` OK | not probed | Release-eligibility checker evaluates the tag regex with case-sensitive Python fullmatch while ci.yml's PowerShell -notmatch is case-insensitive, so the 'exact gate' it claims to test is not the gate that runs. |
| low | F3 | C | independent | [C09](#c09) | `.github/scripts/assert_release_eligibility.py:43` OK | not probed | Checker models GitHub startsWith() with case-sensitive str.startswith; GitHub's expression function is case-insensitive. |
| low | F4 | C | independent | [C06](#c06) | `src/FixPortal.FixAtdl.Wpf/Rendering/WpfControlRenderer.cs:59` OK | not probed | A StrategyPanelRenderer built from a renderer subset throws a bare KeyNotFoundException for a known control type, not the documented NotSupportedException naming type and id. |
| low | F5 | C | independent | [C06](#c06) | `src/FixPortal.FixAtdl.Wpf/AtdlPanel.cs:46` OK | not probed | Public entry point AtdlPanel.Create has no argument guards while every internal Visit uses ThrowIfNull; null strategy surfaces as an NRE deep in rendering. |
| low | F6 | C | independent | [C14](#c14) | `tests/FixPortal.FixAtdl.Wpf.Core.Tests/ViewModels/ControlViewModelTests.cs:57` OK | not probed | Regression test for bool values on CheckBox asserts only NotThrow; it never checks the value reached the control or its parameter. |
| low | F7 | C | independent | [C14](#c14) | `tests/FixPortal.FixAtdl.Wpf.Tests.UI/AtdlPanelTests.cs:647` OK | not probed | XAML-injection-shaped control ids are tested only for NotThrow; nothing asserts the id was encoded or the markup extension not evaluated. |
| low | F8 | C | independent | [C04](#c04) | `src/FixPortal.FixAtdl.Wpf/Rendering/DefaultRendering/CheckBoxRenderer.cs:20` OK | not probed | Required-marker logic (condition, ContentStringFormat, AutomationProperties.Name) is duplicated verbatim in three renderers. |
| low | F9 | C | independent | [C16](#c16) | `tests/FixPortal.FixAtdl.Wpf.Tests.UI/HostThemeInheritanceTests.cs:353` OK | not probed | Test helper Descendants is copy-pasted across UI test files with divergent semantics (visual tree here, logical tree elsewhere) under the same name. |
| low | F10 | C | independent | [C11](#c11) | `.github/scripts/assert_gate_coverage.py:427` OK | not probed | Merge-barrier checker is a 2667-line regex-based YAML parser whose own comments catalogue repeated fail-open parse bugs, though PyYAML is already used by the sibling checker. |
| low | F2 | K | independent | [C04](#c04) | `tests/FixPortal.FixAtdl.Wpf.Tests.UI/AtdlPanelTests.cs:582` OK | not probed | The Descendants visual/logical-tree helper is copied verbatim into 7 UI test files and the Layout(Measure/Arrange/UpdateLayout) triple into 6, instead of living once in a shared test utility (TestStrategies already acknowledges mirroring TestControls for the same reason). |
| low | F3 | K | independent | [C04](#c04) | `src/FixPortal.FixAtdl.Wpf/Controls/DoubleSpinner.xaml.cs:93` OK | not probed | The same up/down KeyDown dispatch is duplicated per button: twice in SingleSpinner, four times in DoubleSpinner (inner/outer x up/down), twice in TimePicker — four near-identical if/else blocks per control where one handler parameterized by (button, direction) would do. |
| low | F4 | K | independent | [C16](#c16) | `tests/FixPortal.FixAtdl.Wpf.Tests.UI/ReviewRegressionTests.cs:16` OK | not probed | Test file ReviewRegressionTests.cs declares 'public partial class AtdlPanelTests', so the class is split across three files one of which cannot be found by class name; combined with the cloned helpers this makes the UI suite's structure the costliest part to navigate. |
| low | F5 | K | supporting F11/C | [C15](#c15) | `stryker-config.json:17` OK | not probed | stryker-config.json sets thresholds.break to 0, so the mutation score can never fail even in the weekly/manual mutation lane; the strongest adequacy signal the repo produces is informational only (deliberate per the plan, but it means weakened assertions would not be caught there). |

### Not named by any criticism in the corpus

The corpus was written about AI-generated code in general, not about this repository. These findings map to none of its claims, which is where the corpus stops bounding the audit.

| Severity | Finding | Seat | Role | Claim | Anchor | Probe | Summary |
|---|---|---|---|---|---|---|---|
| medium | F3 | X | independent | - | `src/FixPortal.FixAtdl.Wpf/Controls/NumericSlider.cs:53` OK | not probed | Selecting a NumericSlider endpoint configured as decimal.MaxValue throws OverflowException because its double representation exceeds decimal range. Map endpoints to their original decimal bounds before converting intermediate values. |
| low | F1 | C | independent | - | `docs/usage.md:21` OK | not probed | docs/usage.md states FixPortal.FixAtdl is pinned at 1.1.4 but Directory.Packages.props pins 1.1.6; the maturity plan's promised doc/version consistency check was never added. |
| low | F11 | C | independent, corroborated by K | - | `stryker-config.json:17` OK | not probed | Mutation lane can never fail: Stryker break threshold is 0 and the summary step omits -FailOnInconclusive, so mutation evidence is advisory only. |
| low | F12 | C | independent | - | `.github/workflows/mutation.yml:20` OK | not probed | mutation.yml checks out without persist-credentials: false, unlike every job in ci.yml and the policy guard. |
| low | F1 | K | independent | - | `docs/usage.md:22` RECOVERED (quote at line 21) | not probed | docs/usage.md states FixPortal.FixAtdl is 'pinned at 1.1.4' but Directory.Packages.props pins 1.1.6; the plan's own Task 4 documentation-consistency check was never added, and CHANGELOG's [Unreleased] never records the 1.1.4->1.1.6 bump. |

## Coverage: every criticism, and what each seat said

One row per published criticism. The verdict column means:

- **confirmed** - a seat found it AND a probe demonstrated it
- **asserted** - a seat found it, with no probe evidence
- **contested** - the seats disagreed; the disagreement is preserved, never averaged
- **clean** - every seat checked it and none found it
- **clean (partial)** - everyone who checked said no, but not everyone checked
- **not assessed** - no seat examined it. This is not a weaker "clean".

The **Looked** column counts how many reporting seats actually examined that criticism. A verdict backed by one seat is weaker evidence than the same verdict backed by all of them, and a bare matrix hides the difference.

| # | Criticism | X | C | K | Looked | Verdict |
|---|---|---|---|---|---|---|
| C02 | AI-generated code reproduces exploitable defects because it was trained on unvetted, buggy code. | not assessed | clean | clean | 2 of 3 | clean (partial) |
| C04 | AI assistance increases duplicated code and reduces refactoring and code reuse. | not assessed | exhibits | exhibits | 2 of 3 | asserted |
| C05 | AI-generated C# ignores nullable reference type annotations: it omits null checks and assigns possibly-null results to non-nullable targets. | clean | clean | clean | 3 of 3 | clean |
| C06 | AI-generated .NET code neglects disposal of resources, reaches for generic exception types, and applies null-checking inconsistently. | not assessed | exhibits | clean | 2 of 3 | contested |
| C07 | AI-generated tests assert general outcomes rather than specific values, and omit coverage for new public methods. | exhibits | clean | clean | 3 of 3 | contested |
| C08 | AI-generated code references packages that do not exist, creating a supply-chain attack surface. | clean | not assessed | clean | 2 of 3 | clean (partial) |
| C09 | This repository's own tests are too few or too weak to falsify the code they cover: they exercise the happy path, assert loosely, or draw inputs from a pool that cannot reach the failing case. | exhibits | exhibits | clean | 3 of 3 | contested |
| C10 | AI-generated code can appear to work while breaking core functionality, revealed only by thorough testing. | exhibits | not assessed | clean | 2 of 3 | contested |
| C11 | AI-generated code solves the immediate task but misses long-term maintainability and architectural fit. | clean | exhibits | clean | 3 of 3 | contested |
| C12 | AI-generated code introduces security flaws at a high rate across major languages, C# included. | not assessed | clean | clean | 2 of 3 | clean (partial) |
| C13 | LLM-generated code carries recurring bug patterns that differ from human-written defects. | not assessed | not assessed | clean | 1 of 3 | clean (partial) |
| C14 | Generated tests run and pass while asserting weakly: they are executable without meaningfully constraining behaviour, so coverage overstates what they verify. | exhibits | exhibits | clean | 3 of 3 | contested |
| C15 | Coverage and mutation adequacy do not catch the faults LLM-generated code actually contains, because the test oracles fail to capture the faulty behaviour. | not assessed | not assessed | exhibits | 1 of 3 | asserted |
| C16 | LLM-generated unit tests carry test smells -- design flaws that undermine readability and maintainability -- beyond what compilability or coverage reveals. | not assessed | exhibits | exhibits | 2 of 3 | asserted |
| C17 | Benchmarks reporting only functional correctness hide the trade-off against maintainability, efficiency and style -- the qualities that decide whether .NET code survives contact with a team. | not assessed | not assessed | clean | 1 of 3 | clean (partial) |

## Practice: disciplines the corpus argues for

These are not allegations against this repository. The corpus carries them because they say why a control exists, and the question is whether this repository follows the discipline - so they are counted nowhere in the verdict above.

- **follows** - the repository demonstrably does this
- **does not follow** - it does not, which is not by itself a defect
- **not assessed** - no seat could tell from source alone

| # | Practice | X | C | K | Looked | Verdict |
|---|---|---|---|---|---|---|
| C20 | Red-green TDD is the working discipline for agent-written code: the agent is given the test command first and made to drive the change from a failing test. | not assessed | not assessed | not assessed | 0 of 3 | not assessed |
| C22 | Static analysis and test feedback fed back into generation measurably improves the code produced, which is the argument for treating analyzers as build-blocking rather than advisory. | follows | follows | follows | 3 of 3 | follows |

### What the matrix does not mean

Every seat audited against the same corpus. That is deliberate - it makes divergence attributable to the model rather than to what each one happened to read - and it means agreement across seats is a **control, not reassurance**. A shared frame produces shared conclusions, so a row of "clean" is evidence about the panel before it is evidence about the code.

## Evidence quality

probe baseline: `dotnet restore "FixPortal.FixAtdl.Wpf.slnx"; dotnet build "FixPortal.FixAtdl.Wpf.slnx" -c Release --no-restore; dotnet test --solution "FixPortal.FixAtdl.Wpf.slnx" -c Release --no-build --timeout 5m` ran green in the clean room, so a probe failure is attributable to its finding

probes: 3 emitted, 3 ran, 2 substantiated, 0 timed out

anchor-validation: 20 checked, 1 marked, 0 not checked (retained, never dropped)

Anchors are checked mechanically: the file must exist at that path, the line must be in range, and any quoted code must match at that line. A finding whose anchor fails is marked in the tables above and kept - deleting it would discard the evidence that settled it.

## How this was produced

### Panel

- **X** (openai) - resolved to `gpt-6-astra` at dispatch
- **C** (anthropic) - resolved to `claude-opus-5-5` at dispatch
- **K** (moonshot) - resolved to `kimi-code/kimi-for-coding` at dispatch

### Clean room

The panel audited an ephemeral git worktree at this commit, not this checkout. The files below were deleted from that worktree before dispatch, so no seat could read a previous audit of this repository and launder it as fresh analysis. They remain in version control here, untouched:

- `docs/2026-09-20-fixportal-fixatdl-wpf-ai-quality-audit.md`

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
