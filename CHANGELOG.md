# Changelog

Notable changes to `FixPortal.FixAtdl.Wpf` and `FixPortal.FixAtdl.Wpf.Core`.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and
this project uses [semantic versioning](https://semver.org/spec/v2.0.0.html).

Dates are the release-tag date, in UTC. Entries are consumer-facing: build, CI
and test-infrastructure commits are omitted unless they change what a consumer
sees. Both packages version together.

## [Unreleased]

### Added

- Diagrams. `docs/usage.md` gains a panel-creation sequence and `docs/api.md`
  a package-split diagram showing that `Wpf.Core` is usable without WPF; the
  README leads with the sequence by absolute URL so it renders on the NuGet
  gallery. Sources are self-contained HTML in `docs/diagrams/`.
- Every source file derived from Atdl4net now carries a one-line attribution to
  Steve Wilkinson's MIT-licensed original. The `NOTICE` already reprinted the
  full upstream grant, but the per-file notices had been removed, which sits
  awkwardly with Apache-2.0 section 4(c). 39 of the 43 source files are derived;
  the four that are not (`AtdlPanel`, `NumericSlider`, `ServiceCollectionExtensions`
  and `AssemblyInfo`) have no upstream counterpart and carry no line.
- This changelog.
- `docs/usage.md` and `docs/api.md` cover host integration, the Wpf/Core
  split, `ReadBackStrategyParametersGrp`, theming, `RenderingException`,
  and the namespaces a host actually imports.

### Changed

- `CONTRIBUTING.md` stated the licence as Apache-2.0, contradicting the package
  SPDX expression `Apache-2.0 AND MIT`. It now states the real position and asks
  contributors to preserve the per-file attribution lines.

### Fixed

- README gains the missing operator docs: custom `IControlRenderer`
  registration and a troubleshooting table (private-feed restore, unknown
  broker controls, one strategy instance per editor, amendment immutability).
  Links are absolute so they resolve on the NuGet gallery, and the 957
  read-back method is named.
- The renderer's visitor fallback threw a bare `NotImplementedException`
  documented "should never get called". It is in fact where an unrecognised
  broker control lands, so a consumer got no message and no control id. It now
  throws `NotSupportedException` naming the control type and its id.
- Partial time entries are preserved while editing rather than being reset.
- Internal failures are preserved rather than swallowed, and empty panel headers
  are named.

### Removed

- `docs/launch/pre-announcement-draft.md`, which was tracked in a public repo
  while headed "Not approved for publication". It remains in git history.

## [1.0.2] - 2026-09-12

### Fixed

- NuGet publishing uses the policy creator for OIDC login.

## [1.0.1] - 2026-09-12

### Added

- `EditViewModel` exposes StrategyParametersGrp (FIX tags 957-960) read-back.

### Changed

- Depends on `FixPortal.FixAtdl` 1.1.0.
- Packages publish to NuGet.org under the FixPortal organization.

## [1.0.0] - 2026-09-12

First public release. WPF rendering and view-model layers for FIXatdl 1.1
strategy panels, over `FixPortal.FixAtdl`.
