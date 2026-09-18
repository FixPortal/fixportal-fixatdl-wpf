# Changelog

Notable changes to `FixPortal.FixAtdl.Wpf` and `FixPortal.FixAtdl.Wpf.Core`.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and
this project uses [semantic versioning](https://semver.org/spec/v2.0.0.html).

Dates are the release-tag date, in UTC. Entries are consumer-facing: build, CI
and test-infrastructure commits are omitted unless they change what a consumer
sees. Both packages version together.

## [Unreleased]

### Added

- A runnable sample: `samples/FixPortal.FixAtdl.Wpf.Sample` renders a synthetic
  "Participate" strategy exercising eleven control types, reads the FIX values
  back, and carries a System/Light/Dark selector so the panel can be watched
  following its host's theme. The strategy document it ships is linked into the
  test project and rendered there, so a sample that stops parsing fails the
  build rather than greeting the next evaluator. The sample calls
  `Strategy_t.LoadInitialControlValues` before `AtdlPanel.Create`, which is
  the documented way to apply each control's `initValue`; without it a
  strategy that pre-populates a field renders that field empty, and a required
  one opens invalid.
  The sample window is sized to the rendered strategy (859px) so the form needs
  no scrolling, and sets an opaque themed background: WPF's Fluent theme puts a
  Mica backdrop on a window, and Mica tints it with the desktop wallpaper's
  dominant colour, which made the sample look as though the library paints forms
  green. The rendered panel is identical either way.
- A required parameter is marked on its label rather than by colour alone. A
  venue document that already marks its own label keeps its marker and gets no
  second one.
- `atdl:ErrorCue.HasErrors`, an attached property that carries the
  invalid-state cue as a local value on a control rather than as an implicit
  style. Rendered `ComboBox` and `ListBox` elements now use it.

### Changed

- The library no longer owns a palette. Chrome resolves through `SystemColors`
  so an embedded panel inherits its host's theme instead of painting over it;
  the inherited `#0046D5` header and `#D0D0BF` border are gone, with hierarchy
  carried by weight. Twenty hardcoded colour literals are down to none.
- Spacing scale: the panel frame's padding was `1`, which left the header
  sitting on the first control and every control against its own border. Panel
  padding is now `8,10,8,8`, sibling panels are `4` apart, and control margins
  moved from `1,3,1,3` to `2,5,2,5`.
- Control labels are vertically centred against their control and separated
  from it by 8px. They were top-aligned with no gap at all.

### Fixed

- A required field was tinted `MistyRose`. Under a themed host that pinned
  light-theme colour was carried into the `ComboBox` dropdown while the theme
  supplied a near-white foreground, leaving the list unreadable. The tint is
  gone entirely; the label marker carries the requirement.
- **An open dropdown is now readable under a dark host theme.** The library
  declared implicit styles for `ComboBox`, `ListBox` and `ItemsControl`,
  and its resource dictionary is merged into the innermost resource scope in the
  panel's tree - so those styles shadowed the host's own implicit styles
  entirely, for every property, not just the one a trigger touched. Measured
  under WPF's Fluent dark theme: the `ComboBox` fell back to the legacy
  template (light chrome, `#FF000000` text) while its items, which the
  dictionary said nothing about, kept Fluent's `#FFFFFFFF` foreground. All
  three styles are gone. The invalid-state cue they carried is now `ErrorCue`,
  which sets one property and clears it; the `ItemsControl` style was dead,
  because nothing this library renders is one.
- **Text inputs follow a dark host too.** Two separate causes, both measured.
  `ClickSelectTextBox` derives from `TextBox`, and WPF matches an implicit
  style on the element's EXACT type, so it never picked up a host's `TextBox`
  style: under Fluent dark it rendered `#FFFFFFFF` chrome with `#FF000000`
  text while a plain `TextBox` beside it was `#0FFFFFFF` on `#FFFFFFFF`.
  It now points its own `Style` at that resource, and falls back to its theme
  style unchanged where a host declares none. Separately, the spinner and clock
  templates pinned `SystemColors.WindowBrushKey` and
  `SystemColors.ControlBrushKey` for their input surface and buttons; WPF's
  Fluent theme does not redefine the legacy `SystemColors`, so those stayed
  white in dark mode. They now take their surface and their arrow colour from
  their own inner text box, which means one source of truth and no pinned
  colour. `LightSlateGray`, the last named colour literal in the dictionary,
  is gone with them.
- A `Border` in the multi-select template set `BorderBrush="Blue"` with no
  thickness - inert today, a pure blue ring the moment anyone set one.


## [1.0.3] - 2026-09-17

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

- Takes `FixPortal.FixAtdl` 1.1.4, up from 1.1.2. Consumer-visible effects of
  that move: exponent spellings (`1E2`) in FIX field values are rejected rather
  than read as the expanded number; a control's `Id` can no longer be changed
  once it belongs to a panel; a duplicate control id is rejected by the panel's
  own collection at insertion, earlier than the renderer's guard; and a
  `Clock_t` the adapter can edit must carry a `localMktTz`, because a bare
  time-of-day has no way to become an instant without a zone.

- `CONTRIBUTING.md` stated the licence as Apache-2.0, contradicting the package
  SPDX expression `Apache-2.0 AND MIT`. It now states the real position and asks
  contributors to preserve the per-file attribution lines.

### Fixed

- Two defects the composition review found in the path the exclusion below opens.
  `RefreshRules` cleared its `_refreshing` re-entrancy guard only after its
  `finally` had swept every control-less parameter's `WireValue`, so an
  exception escaping that sweep latched the guard forever: rules silently
  stopped applying for the lifetime of the view model while `HasErrors` still
  read clean. And validation writes the control before its parameter, so an
  exception escaping mid-way left the two disagreeing with nothing recorded -
  `ReadBackFixValues` then emitted the parameter's stale wire value behind a
  clean `HasErrors`, which is a silently wrong order value. The guard now
  clears on every path, and a torn write is latched so a read-back fails closed.
- An `InternalErrorException` raised by the core was being reported to the user
  as a field validation error and swallowed. `IsValidationException` matches
  `FixAtdlException`, and `FixPortal.FixAtdl` 1.1.3 re-parented
  `InternalErrorException` from `Exception` onto `FixAtdlException`, so the
  existing type list started catching it. A broken library invariant is not user
  input; it is now excluded explicitly and propagates to the host again.
- The five XAML files derived from Atdl4net carried no attribution line while
  every derived `.cs` file did. `Controls/Slider.xaml`,
  `Controls/DoubleSpinner.xaml`, `Controls/SingleSpinner.xaml`,
  `Controls/TimePicker.xaml` and `FixAtdlWpfResources.xaml` all have exact
  upstream counterparts and now carry the same notice the code files do.
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
