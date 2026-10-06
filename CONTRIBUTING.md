# Contributing

Issues and pull requests are welcome. This repo provides WPF controls that
render strategy definitions produced by the headless `FixPortal.FixAtdl`
package.

## Getting set up

The build consumes the public `FixPortal.CodeStyle` package from NuGet.org.
Contributor restore, build, test, pack, and vulnerability scanning require no
FixPortal credentials.

Then the standard loop:

```powershell
dotnet tool restore
```

```powershell
dotnet csharpier format .
```

```powershell
dotnet restore FixPortal.FixAtdl.Wpf.slnx
```

```powershell
dotnet build FixPortal.FixAtdl.Wpf.slnx --configuration Release --no-restore
```

```powershell
dotnet test --solution FixPortal.FixAtdl.Wpf.slnx --configuration Release --no-build
```

CI runs `dotnet csharpier check .`, which validates formatting without
rewriting files — run `dotnet csharpier format .` before pushing.

## Conventions

- **Public API.** The package's public surface is a compatibility contract;
  call out any breaking signature or behaviour change explicitly in the PR.
- **Tests.** xUnit v3, AwesomeAssertions (`.Should()`), NSubstitute.
- **Licence.** `Apache-2.0 AND MIT`. New code is Apache-2.0; the WPF control,
  rendering and view-model layers derive from Steve Wilkinson's MIT-licensed
  Atdl4net and carry a per-file attribution line. Keep that line when you edit
  a file that has one. See `NOTICE` for the full upstream grant.

## Pull requests

- Branch from `main`, open a PR, and let CI go green before asking for
  review. The required checks are `CI Gate` (build, test, CSharpier,
  actionlint) and `Review policy intact`.
- Some files under `.github/` are shared CI assets synced from FixPortal's
  internal tooling (listed in `.github/canonical-assets.json`); CI rejects
  local edits to them. If one needs changing, say so in an issue or PR
  description and a maintainer will make the change upstream.
- PRs are merged rebase-only; keep commits clean and individually meaningful.
