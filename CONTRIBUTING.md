# Contributing

Issues and pull requests are welcome. This repo provides WPF controls that
render strategy definitions produced by the headless `FixPortal.FixAtdl`
package.

## Getting set up

The build consumes `FixPortal.CodeStyle` from the private FixPortal GitHub
Packages feed, so restore needs a token with `read:packages` on the `FixPortal`
org exported first:

```powershell
$env:GITHUB_PACKAGES_TOKEN = "<token with read:packages>"
```

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
- **Licence.** Apache-2.0.

## Pull requests

- Branch from `main`, open a PR, and let CI (build, test, CSharpier,
  actionlint) go green before asking for review.
- PRs are merged rebase-only; keep commits clean and individually meaningful.
