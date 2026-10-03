## Summary

Describe the user-visible or repository-level change and why it is needed.

## Validation

- [ ] `dotnet csharpier check .`
- [ ] `dotnet build FixPortal.FixAtdl.Wpf.slnx --configuration Release --no-restore`
- [ ] `dotnet test --solution FixPortal.FixAtdl.Wpf.slnx --configuration Release --no-build`
- [ ] Applicable package, workflow, or documentation checks passed.

## Review checklist

- [ ] Documentation is updated for changed behaviour, public API, package metadata, or release process.
- [ ] Breaking API or FIX wire-format impact is called out explicitly.
- [ ] Tests assert the important values and failure modes, not only that execution completes.
- [ ] License and Atdl4net attribution requirements are preserved.
- [ ] No secrets, proprietary strategy documents, or client data are included.
