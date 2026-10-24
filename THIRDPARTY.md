# Third-Party Notices

This package wraps the following third-party projects for use with Unity's test framework.

## ArchUnit

- Website: https://www.archunit.org/
- Repository: https://github.com/TNG/ArchUnit
- License: Apache License 2.0

The original Java library this package's API and concepts are based on.

## ArchUnitNET

- Repository: https://github.com/TNG/ArchUnitNET (published to NuGet as `TngTech.ArchUnitNET`;
  the bare `ArchUnitNET` package id on NuGet.org belongs to an unrelated project)
- License: Apache License 2.0

The .NET port of ArchUnit this package wraps. `StronglyConnectedComponents.dll` is .NET Standard
and shared unchanged by both scripting backends. `ArchUnitNET.dll` itself ships as two separate
builds instead, one per backend (see `Plugins/CoreCLR/ArchUnitNET.dll` below) — never a single
DLL patched to work both ways.

ArchUnitNET has a hard runtime dependency on Mono.Cecil for reading assembly metadata. Under Mono,
this has always been satisfied implicitly by Unity's own `com.unity.nuget.mono-cecil` package —
`Plugins/ArchUnitNET.dll` is the unmodified upstream build and still relies on that. That package
is absent from Unity 7 projects, which under CoreCLR surfaced as Unity flagging `ArchUnitNET.dll`
itself as a "broken assembly" at code-reload time (confirmed against a live Unity 7 alpha Editor).

## Mono.Cecil (merged into Plugins/CoreCLR/ArchUnitNET.dll)

- Repository: https://github.com/jbevain/cecil
- License: MIT

`Mono.Cecil.dll` and `Mono.Cecil.Rocks.dll`, exact version `0.11.6` (read straight from
`ArchUnitNET.dll`'s own AssemblyRef table) — the version ArchUnitNET already depends on. Rather
than vendor these as loose DLLs into the project for CoreCLR, `Plugins/CoreCLR/ArchUnitNET.dll` is
a separate build produced from the unmodified upstream `ArchUnitNET.dll` with Mono.Cecil and
Mono.Cecil.Rocks merged directly into it via [ILRepack](https://github.com/gluck/il-repack) (MIT
license) with `/internalize`, so the result carries no external `Mono.Cecil` reference at all:

```
ILRepack /out:ArchUnitNET.dll /internalize ArchUnitNET.dll Mono.Cecil.dll Mono.Cecil.Rocks.dll
```

Verified functionally equivalent to the unmerged build outside Unity (same classes discovered,
same rule pass/fail results) before vendoring. `PluginImporter` `defineConstraints` on each
`ArchUnitNET.dll.meta` pick exactly one of `Plugins/ArchUnitNET.dll` (Mono) or
`Plugins/CoreCLR/ArchUnitNET.dll` (CoreCLR) at a time — see the README's "CoreCLR support" section.

## System.Collections.Immutable

- Repository: https://github.com/dotnet/runtime
- License: MIT

A runtime dependency of ArchUnitNET. Mono's base class library doesn't ship this assembly, so
`Plugins/System.Collections.Immutable.dll` (an older build) is bundled for it. CoreCLR ships this
assembly inbox, which can conflict with a second, separately-loaded copy of the same simple name,
so a second, newer .NET Standard 2.0 build lives at `Plugins/CoreCLR/System.Collections.Immutable.dll`
instead. `PluginImporter` `defineConstraints` on each `.dll.meta` pick exactly one variant at a time
(see `ScriptingBackend.cs` and the README's "CoreCLR support" section) — whether this particular
assembly conflicts under CoreCLR the way Mono.Cecil did is still **not yet verified against a live
CoreCLR Editor**.
