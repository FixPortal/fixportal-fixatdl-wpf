# Security Policy

## Reporting a vulnerability

Please report suspected vulnerabilities privately via
[GitHub Security Advisories](https://github.com/FixPortal/fixportal-fixatdl-wpf/security/advisories/new)
rather than opening a public issue.

You should receive an acknowledgement within a few days. Please give us a
reasonable window to investigate and ship a fix before any public disclosure.

## Attack surface

`FixPortal.FixAtdl.Wpf` is a WPF control library that renders FIXatdl strategy
UIs described by `FixPortal.FixAtdl` model objects. Its security-relevant
surface is:

- **Layout and binding of externally-supplied strategy definitions** — a
  malformed or hostile strategy (parsed upstream by `FixPortal.FixAtdl`) that
  reaches this library's layout/render path.
- **User input captured by generated controls** — values entered into
  rendered controls flow back into FIX-tag emission via `FixPortal.FixAtdl`;
  a value that is silently wrong is more dangerous than one that fails
  loudly.

The renderer composes XAML with an XML writer and parses it with
`XamlReader.Parse`. XML escaping prevents element injection; literal XAML
escaping prevents venue labels and titles from becoming markup extensions.
Control names are encoded, binding indexes are generated from the control
collection, and duplicate or empty IDs are rejected before panel creation.
Custom renderers are trusted application code and must preserve these boundaries.

The adapter performs no network I/O. Strategy XML parsing and parameter wire
conversion belong to `FixPortal.FixAtdl`. Invalid edits are exposed through
`HasErrors`; FIX read-back refuses a strategy with validation errors.

## Supported versions

| Version | Supported |
|---|---|
| Latest release | Yes |
| Older releases | No |
