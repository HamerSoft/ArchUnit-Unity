# Third-Party Notices

This package wraps the following third-party projects for use with Unity's test framework.

## ArchUnit

- Website: https://www.archunit.org/
- Repository: https://github.com/TNG/ArchUnit
- License: Apache License 2.0

The original Java library this package's API and concepts are based on.

## ArchUnitNET

- Repository: https://github.com/TNG/ArchUnitNET
- License: Apache License 2.0

The .NET port of ArchUnit (Mono.Cecil-based) bundled in `Plugins/` and wrapped by this package.
`ArchUnitNET.dll` and `StronglyConnectedComponents.dll` target .NET Standard and are shared by
both the Mono and CoreCLR scripting backends.

## System.Collections.Immutable

- Repository: https://github.com/dotnet/runtime
- License: MIT

A runtime dependency of ArchUnitNET. Mono's base class library doesn't ship this assembly, so
`Plugins/System.Collections.Immutable.dll` (an older build) is bundled for it. CoreCLR ships this
assembly inbox, which can conflict with a second, separately-loaded copy of the same simple name,
so a second, newer .NET Standard 2.0 build lives at `Plugins/CoreCLR/System.Collections.Immutable.dll`
instead. `PluginImporter` `defineConstraints` on each `.dll.meta` pick exactly one variant at a time
(see `ScriptingBackend.cs` and the README's "CoreCLR support" section) — this split is prepared but
**not yet verified against a live CoreCLR Editor**.
