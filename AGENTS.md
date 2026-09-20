# AGENTS.md — FixPortal.FixAtdl.Wpf

Repo-specific conventions for agent work in this repository. Global FixPortal
rules apply on top of these; this file only carries what is true here and
nowhere else.

## What this repo is

A WPF control library that renders FIXatdl strategy UIs from the model
objects produced by the headless `FixPortal.FixAtdl` package
(`FixPortal/fixportal-fixatdl`). Two-project split: `FixPortal.FixAtdl.Wpf`
(`net10.0-windows`, WPF controls/XAML) and `FixPortal.FixAtdl.Wpf.Core`
(`net10.0`, ViewModels and non-WPF logic — layout coordinate math,
validation).

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
