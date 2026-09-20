# FixAtdl WPF OSS Maturity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bring `fixportal-fixatdl-wpf` from consumer-ready to a fully complete, mature, and house-conformant public OSS repository.

**Architecture:** Preserve the existing two-package split (`FixPortal.FixAtdl.Wpf.Core` for platform-independent editing and `FixPortal.FixAtdl.Wpf` for WPF rendering). Make the smallest changes at the repository and release boundaries: contributor bootstrap, GitHub controls, package metadata, documentation truth, and measurable quality gates. Do not refactor stable rendering or view-model code unless a new check demonstrates a release-blocking defect.

**Tech Stack:** .NET 10, WPF, C# nullable reference types, central NuGet package management, CSharpier 1.3.0, xUnit v3, AwesomeAssertions, NSubstitute, GitHub Actions, NuGet.org, GitHub Packages.

**Spec:** Repository-wide OSS/documentation sweep performed 2026-09-20; current house rules in `scaffold-ci`, `scaffold-dotnet`, `scaffold-doc`, and the repository `AGENTS.md`.

## Global Constraints

- Preserve `net10.0` for Core and `net10.0-windows` for WPF and sample projects.
- Keep `.slnx`, `src/`, `tests/`, central package management, nullable, implicit usings, CSharpier, and the existing package split.
- Use canonical assets from `C:\Users\chris\.agents\skills\scaffold-ci\assets`; do not hand-retype house CI controls.
- Do not add a public dependency on internal implementation details or change the public API without an explicit compatibility decision.
- Public repositories must not require FixPortal credentials for restore, build, test, pack, or vulnerability scanning.
- New tests use the existing xUnit v3 + AwesomeAssertions + NSubstitute stack.
- Every PR runs restore, CSharpier check, Release build, full test suite, and applicable workflow/package validation.
- Configuration and review-control changes follow the repository's HIGH review tier; documentation-only changes remain LOW only if they are not shipped package content.
- Do not claim vulnerability-free status when the private-feed limitation prevents the dependency audit from running.

## Review Focus

- A contributor without FixPortal credentials can clone, restore, build, test, and pack the repository using documented public dependencies or an explicitly documented public bootstrap path.
- A pull request cannot bypass CI, review-policy protection, or secret scanning by editing the repository's own workflows and checkers.
- A package consumed from NuGet contains the README, license, NOTICE, dependencies, and version information that the README and API docs describe.
- The documented `FixPortal.FixAtdl` version, package versions, supported target frameworks, public entry points, and failure modes match the source and generated packages.
- Coverage and security checks report their actual scope and fail or remain explicitly unassessed when required evidence is unavailable.

### Task 1: Freeze the house-rule baseline and split the rollout

**Files:**
- Inspect: `AGENTS.md`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, `.gitattributes`, `.csharpierignore`, `.config/dotnet-tools.json`, `nuget.config`
- Inspect: `.github/workflows/ci.yml`, `.github/workflows/review-policy-guard.yml`, `.github/scripts/assert_gate_coverage.py`, `.github/scripts/assert_workflow_hygiene.py`, `.claude/review-policy.json`, `.coderabbit.yaml`, `.github/dependabot.yml`
- Create: `docs/superpowers/plans/2026-09-20-fixatdl-wpf-oss-maturity.md` (this plan)

**Interfaces:**
- Produces a checked house-rule ledger that later tasks use as the acceptance baseline.
- Does not alter product code or public API.

- [ ] **Step 1: Record the current repository state**

  Run:

  ```powershell
  git status --short --untracked-files=all
  git branch --show-current
  git rev-parse HEAD
  ```

  Preserve the existing README/image edits, audit report, and review worktree. Do not reset, stash, or clean them.

- [ ] **Step 2: Compare tracked control-plane files with canonical house assets**

  Compare `.github/workflows/review-policy-guard.yml`, `.github/scripts/assert_gate_coverage.py`, `.github/scripts/assert_workflow_hygiene.py`, and `.claude/review-policy.json` against the corresponding scaffold assets. Record byte-identical files and intentional repository-specific differences before editing.

- [ ] **Step 3: Run the decisive baseline checks**

  ```powershell
  dotnet tool restore
  dotnet csharpier check .
  dotnet restore FixPortal.FixAtdl.Wpf.slnx
  dotnet build FixPortal.FixAtdl.Wpf.slnx --configuration Release --no-restore
  dotnet test --solution FixPortal.FixAtdl.Wpf.slnx --configuration Release --no-build
  ```

  Record the private-feed authentication requirement separately from product failures.

- [ ] **Step 4: Commit only the plan if the plan itself is being adopted**

  ```powershell
  git add docs/superpowers/plans/2026-09-20-fixatdl-wpf-oss-maturity.md
  git commit -m "docs: plan OSS maturity work"
  ```

### Task 2: Normalize missing house CI and repository controls

**Files:**
- Verify GitHub: Secret Scanning and Push Protection settings for this public repository
- Update if required by canonical comparison: `.github/workflows/review-policy-guard.yml`
- Update if required by canonical comparison: `.github/scripts/assert_gate_coverage.py`
- Update if required by canonical comparison: `.github/scripts/assert_workflow_hygiene.py`
- Update: `.claude/review-policy.json`
- Server settings: GitHub `main` branch protection or repository ruleset
- Create: `.github/ISSUE_TEMPLATE/bug-report.yml`, `.github/ISSUE_TEMPLATE/feature-request.yml`
- Create: `.github/pull_request_template.md`
- Create if ownership is agreed: `.github/CODEOWNERS`

**Interfaces:**
- `CI Gate` remains the required aggregate status for the mainline.
- `Review policy intact` remains the exact required job name.
- Public-repository secret scanning and push protection are enabled without adding a private-repository TruffleHog sweep.

- [ ] **Step 1: Verify public-repository secret protection**

  Query GitHub's effective Secret Scanning and Push Protection settings for `FixPortal/fixportal-fixatdl-wpf`. Enable them if the account has authority, or record the exact server-side handoff. Do not copy `C:\Users\chris\.agents\skills\scaffold-ci\assets\secret-sweep.yml`: that canonical TruffleHog asset is explicitly scoped to private repositories, while this repository is public.

- [ ] **Step 2: Keep the review policy focused on repository control code**

  Do not add a nonexistent secret-sweep workflow to HIGH. Keep dependency manifests out of HIGH and retain `docs/**` as LOW only after confirming docs are not independently deployed.

- [ ] **Step 3: Add contributor-facing GitHub templates**

  Keep templates short and operational: reproduction/environment/expected-result for bugs; use case and compatibility impact for features; checklist entries for tests, docs, breaking API, and licensing/attribution.

- [ ] **Step 4: Configure mainline protection**

  In GitHub, require `CI Gate` and `Review policy intact`, require at least one approving review, dismiss stale approvals, require branches to be up to date, block force-push and deletion, and require conversation resolution. If organisation policy already supplies these rules, record the rule source and verify the effective result rather than duplicating it.

- [ ] **Step 5: Validate the control plane**

  ```powershell
  python .github/scripts/assert_workflow_hygiene.py
  python .github/scripts/assert_gate_coverage.py .github/workflows/ci.yml
  git add --dry-run .claude/review-policy.json
  ```

  Also run actionlint using the repository's CI action and inspect the effective GitHub ruleset and secret-protection settings after the server-side changes.

- [ ] **Step 6: Commit the configuration slice**

  ```powershell
  git add .github .claude/review-policy.json
  git commit -m "ci: complete OSS repository controls"
  ```

### Task 3: Remove the contributor bootstrap blocker

**Files:**
- Coordinate with: `FixPortal/fixportal-codestyle` package/repository
- Update: `Directory.Packages.props` only if the public package identity/version changes
- Update: `Directory.Build.props` only if the shared reference shape changes
- Update: `nuget.config` if a public source mapping is introduced
- Update: `CONTRIBUTING.md`, `README.md`
- Add: a documented clean-room bootstrap check under `.github/scripts/` only if it is needed to prove the public path

**Interfaces:**
- Preferred outcome: this repository consumes a public portable `FixPortal.CodeStyle` package, while internal-only rules move to a separate private `FixPortal.CodeStyle.Internal` package.
- `FixPortal.FixAtdl` is already published on NuGet.org; this repository must stop routing that public package through the private feed once the CodeStyle dependency is public.
- Internal repositories may consume both tiers; this public repository must consume only public package sources.

- [ ] **Step 1: Define the two CodeStyle tiers**

  Keep the stable package identity `FixPortal.CodeStyle` for the portable, public rules so consuming repositories do not need a coordinated package-ID migration. Create `FixPortal.CodeStyle.Internal` for rules that expose internal architecture, organisation policy, proprietary paths, or other material not intended for public distribution.

- [ ] **Step 2: Classify and test the CodeStyle contents**

  In `FixPortal/fixportal-codestyle`, classify every global rule, bundled analyzer, suppression, and documentation asset as public or internal. Verify redistribution terms for bundled analyzers and confirm the public package contains no secrets, internal URLs, private repository names, or organisation-only assumptions. Add package-consumer tests for both tiers: a public fixture restores without credentials; an internal fixture proves the private overlay remains enforced where intended.

- [ ] **Step 3: Publish the public package and migrate internal consumers**

  Publish the portable `FixPortal.CodeStyle` package to NuGet.org with the existing versioned release process. Move internal-only rules into `FixPortal.CodeStyle.Internal` on the authenticated FixPortal feed. Update internal repositories to reference the overlay explicitly; do not make internal rules appear magically in the public package.

- [ ] **Step 4: Remove private-feed routing from this public repository**

  After the public package is available, update `nuget.config` so this repository restores `FixPortal.CodeStyle` and `FixPortal.FixAtdl` from NuGet.org without querying the private feed. Remove the private source and credential block unless another genuinely private dependency remains. Keep `Directory.Build.props`'s versionless `PrivateAssets="all"` CodeStyle reference and update only the central version.

- [ ] **Step 5: Test a clean-room restore**

  Run restore/build/test/pack with no `GITHUB_PACKAGES_TOKEN` and with empty local NuGet caches in an isolated checkout. The acceptance condition is successful restore, build, test, pack, and `dotnet list package --vulnerable --include-transitive`; a helpful authentication error is not a pass.

- [ ] **Step 6: Update contributor instructions**

  Document the no-token path first. Remove the mandatory token setup from this repository's normal loop. Link to the internal CodeStyle repository only for maintainers working on the private overlay, and state that this repository does not require it.

- [ ] **Step 7: Validate package dependency closure**

  ```powershell
  dotnet pack src/FixPortal.FixAtdl.Wpf.Core -c Release --no-build -o artifacts/oss-check
  dotnet pack src/FixPortal.FixAtdl.Wpf -c Release --no-build -o artifacts/oss-check
  ```

  Inspect both nuspec files and confirm neither `FixPortal.CodeStyle` nor `FixPortal.CodeStyle.Internal` is a runtime dependency.

- [ ] **Step 8: Commit the contributor bootstrap slice**

  ```powershell
  git add Directory.Build.props Directory.Packages.props nuget.config CONTRIBUTING.md README.md .github/scripts
  git commit -m "build: make OSS contributor bootstrap self-service"
  ```

### Task 4: Make documentation current, complete, and package-aware

**Files:**
- Update: `docs/usage.md`
- Update: `docs/api.md`
- Update: `README.md`
- Update: `CHANGELOG.md`
- Update if needed: `SECURITY.md`, `CONTRIBUTING.md`
- Optional create: `docs/compatibility.md`
- Optional create: `docs/release.md`

**Interfaces:**
- Documentation describes the current package versions, target frameworks, public entry points, renderer extension points, failure modes, theming, and release/support policy.
- The package README remains renderable by NuGet and does not depend on vault-only metadata.

- [ ] **Step 1: Correct known stale facts**

  Change the `FixPortal.FixAtdl` version in `docs/usage.md` from 1.1.2 to the actual centrally pinned version 1.1.4.

- [ ] **Step 2: Define the supported public surface**

  Choose one explicit policy: either document every public type/member intended for consumers, or mark implementation-public generated control types and rendering helpers as unsupported/internal-by-convention. Make `docs/api.md` match that policy.

- [ ] **Step 3: Add compatibility and lifecycle facts**

  State the supported .NET/Windows baseline, package versioning relationship, release source of truth, upstream Atdl4net attribution boundary, and how breaking changes are announced.

- [ ] **Step 4: Add executable documentation checks**

  Add a small repository check that asserts the documented `FixPortal.FixAtdl` version equals `Directory.Packages.props`, both package IDs are present, all documented local links exist, and README/package README references resolve to tracked assets or stable public URLs.

- [ ] **Step 5: Validate the docs against generated packages**

  Pack both libraries, extract the README, LICENSE, NOTICE, and nuspec files, and compare their version, dependency, license, and framework claims with the docs.

- [ ] **Step 6: Commit the documentation slice**

  ```powershell
  git add README.md docs CHANGELOG.md SECURITY.md CONTRIBUTING.md
  git commit -m "docs: complete OSS usage and API contract"
  ```

### Task 5: Raise measurable quality maturity without broad refactoring

**Files:**
- Update: `.github/workflows/ci.yml`
- Create only if justified by the test audit: `.github/workflows/mutation.yml`, `stryker-config.json`
- Update: `Directory.Packages.props` only for deliberately approved test tooling
- Add tests beside the behaviours they protect under `tests/`
- Optional create: `docs/quality.md`

**Interfaces:**
- Required PR CI remains bounded and fast.
- Extended coverage and mutation testing run manually/weekly, never as an unbounded PR gate.
- Coverage numbers describe scope and do not replace behavioural assertions.

- [ ] **Step 1: Run a focused test-adequacy review**

  Map each public host workflow to tests: strategy load, panel creation, every registered control type, validation/read-back, state rules, amendment behaviour, theme inheritance, custom renderer registration, malformed input, and package resource loading.

- [ ] **Step 2: Add only high-value missing tests**

  Use parameterized tests for control families and UI smoke tests for WPF/resource seams. Demonstrate each new regression test fails before the fix when a defect is found.

- [ ] **Step 3: Keep coverage measurement honest**

  Preserve the current Core coverage artifact, add a published scope statement, and add thresholds only after measuring a stable baseline. Do not add a threshold that can be gamed by generated/XAML plumbing.

- [ ] **Step 4: Add mutation testing only if the adequacy review finds oracle risk**

  If mutation testing is warranted, copy the canonical Stryker configuration/template and schedule it weekly/manual with a 45-minute cap. Do not put it in the required PR lane.

- [ ] **Step 5: Validate quality gates**

  ```powershell
  dotnet csharpier check .
  dotnet build FixPortal.FixAtdl.Wpf.slnx --configuration Release --no-restore
  dotnet test --solution FixPortal.FixAtdl.Wpf.slnx --configuration Release --no-build
  ```

- [ ] **Step 6: Commit the quality slice**

  ```powershell
  git add .github/workflows/ci.yml .github/workflows/mutation.yml stryker-config.json Directory.Packages.props tests docs/quality.md
  git commit -m "test: make release quality evidence measurable"
  ```

### Task 6: Release and maturity closeout

**Files:**
- Update: `.github/workflows/ci.yml` only for verified release-control gaps
- Update: `CHANGELOG.md`
- Optional create: `.github/RELEASE_TEMPLATE.md`
- Optional create: `docs/release.md`

**Interfaces:**
- A tag reachable from `main` produces the validated pair of packages with matching versions and provenance.
- GitHub release notes, NuGet package metadata, README, and changelog agree on the current release.

- [ ] **Step 1: Align release source of truth**

  Decide whether GitHub releases and NuGet releases intentionally differ. If not, publish the missing GitHub release for the current NuGet version; if yes, document the distinction.

- [ ] **Step 2: Perform a release rehearsal**

  Validate tag ancestry, package version selection, package contents, README rendering, license/NOTICE inclusion, and NuGet/GitHub package publication using a non-production or dry-run-compatible path.

- [ ] **Step 3: Record support and deprecation policy**

  Ensure `SECURITY.md`, changelog, README, and release notes agree on the supported version and disclosure route.

- [ ] **Step 4: Run the final maturity gate**

  Require:

  - clean contributor restore/build/test path;
  - required GitHub rules and green `CI Gate`;
  - secret scanning and workflow hygiene green;
  - package contents validated;
  - documentation consistency check green;
  - full local Release build/test and CSharpier check green;
  - no unresolved HIGH findings.

- [ ] **Step 5: Close with one reviewable status record**

  Append the final evidence, commit SHAs, package version, GitHub ruleset verification, and any explicitly accepted residual risks to the project’s maturity report. Do not overwrite the prior audit baseline.

## Proposed PR order

1. **House controls:** canonical CI/security/review assets, templates, and effective `main` protection.
2. **Contributor bootstrap:** remove or isolate the private-feed blocker.
3. **Documentation truth:** version correction, API/support contract, and consistency checks.
4. **Quality maturity:** only evidence-backed tests, coverage scope, and optional weekly mutation lane.
5. **Release closeout:** align release surfaces and complete the final gate.

The first three PRs are required for “fully complete.” The fourth is maturity work whose size should be determined by the test-adequacy evidence; the fifth is the release sign-off rather than a reason to hold ordinary development hostage.

## Self-review

- The plan covers the known OSS blockers: private restore dependency, absent effective branch protection, missing secret-sweep control, missing contribution templates, stale dependency documentation, and incomplete public API documentation.
- The CodeStyle remediation is now explicit: public portable rules remain under `FixPortal.CodeStyle`; internal-only rules move to `FixPortal.CodeStyle.Internal`; this repository removes private-feed routing once the public package is available.
- It preserves the existing application architecture and does not introduce speculative abstractions.
- Every proposed implementation slice has a repository-level validation command and a separate commit boundary.
- The plan does not claim the private-feed issue can be solved entirely inside this repository; the package distribution decision is an explicit prerequisite.
