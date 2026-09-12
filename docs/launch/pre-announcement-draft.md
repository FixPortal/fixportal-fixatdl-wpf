# Pre-announcement draft — not approved for publication

**Status: source draft only.** This unlinked file is publicly visible in the
repository, but it has not been approved for publication. Do not treat it as a
release announcement or publish it without approval.

## Draft copy

`FixPortal.FixAtdl.Wpf` is a .NET 10 WPF adapter for editing FIXatdl strategy
forms from [`FixPortal.FixAtdl`](https://www.nuget.org/packages/FixPortal.FixAtdl/)
models. Install it from NuGet.org with:

```sh
dotnet add package FixPortal.FixAtdl.Wpf
```

The package includes the platform-independent
[`FixPortal.FixAtdl.Wpf.Core`](https://www.nuget.org/packages/FixPortal.FixAtdl.Wpf.Core/)
layer for value initialization, state rules and validation. The WPF adapter
renders the form; the host application owns its window, lifecycle and order
submission.

The companion React adapter is a separate npm package,
[`@fix-portal/fixatdl-react`](https://www.npmjs.com/package/@fix-portal/fixatdl-react),
with its own [repository](https://github.com/FixPortal/fixportal-fixatdl-react).

This is not a FIX engine, an order-management system, or a claim of complete
FIXatdl conformance. Hosts remain responsible for message construction,
instrument data, order submission and cancel/replace policy.

The WPF control and layout code derives from Steve Wilkinson's MIT-licensed
Atdl4net implementation (2010–2011). See [NOTICE](../../NOTICE) and
[LICENSE](../../LICENSE).
