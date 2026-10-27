# AGENTS.md — FixPortal.FixAtdl.Wpf

Repo-specific conventions for agent work in this repository. Global FixPortal
rules apply on top of these; this file only carries what is true here and
nowhere else.

## What this repo is

A WPF control library that renders FIXatdl strategy UIs from the model
objects produced by the headless `FixPortal.FixAtdl` package
(`FixPortal/fixportal-fixatdl`). Two-project split: `FixPortal.FixAtdl.Wpf`
(`net10.0-windows`, WPF controls/XAML) and `FixPortal.FixAtdl.Wpf.Core`
(`net10.0`, ViewModels and non-WPF logic — validation).

## Build, format, test

- CSharpier is pinned in `.config/dotnet-tools.json`; restore with
  `dotnet tool restore`, check with `dotnet csharpier check .` (CI runs the
  read-only check; format locally).
- Tests: `tests/FixPortal.FixAtdl.Wpf.Core.Tests` (xUnit v3 + AwesomeAssertions
  + NSubstitute) and `tests/FixPortal.FixAtdl.Wpf.Tests.UI` (STA smoke tests
  for WPF controls). Assert with `.Should()`, never xUnit `Assert.*`.
  Run both with `dotnet test --solution FixPortal.FixAtdl.Wpf.slnx --configuration Release`.

## Package restore

All current package dependencies restore from NuGet.org. Contributor restore,
build, test, pack, and vulnerability scanning do not require FixPortal
credentials.

## Review workflow

PRs merge rebase-only. See `.claude/review-policy.json` for risk tiering.

## Release eligibility and local guard controls

The publish job in `.github/workflows/ci.yml` peels `GITHUB_SHA` to a commit and
asserts reachability from `origin/main` with `git merge-base --is-ancestor`
before publishing. `.github/scripts/assert_release_eligibility.py` verifies
the workflow's release wiring. Together these are this repository's local
release-eligibility implementation; preserve the fail-closed publish boundary
and its existing regression coverage when normalizing workflow syntax.

The review-policy guard deliberately protects that release-wiring checker as
a HIGH control. Its adapted content is recorded in
`.github/canonical-assets.json`; keep this protection when syncing the guard.
