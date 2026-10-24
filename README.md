# ArchUnit-Unity

[ArchUnitNET](https://github.com/TNG/ArchUnitNET) wrapped for Unity's test framework, so you can write architecture tests (layering rules, dependency constraints, naming conventions, ...) as regular EditMode tests.

## Requirements

- Unity 2021.2+ (.NET Standard 2.1 API compatibility)
- Mono or CoreCLR scripting backend (see [CoreCLR support](#coreclr-support) below)

## Install

Add via Package Manager → Add package from git URL:

```
https://github.com/HamerSoft/ArchUnit-Unity.git
```

## Usage

```csharp
using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using HamerSoft.ArchUnitUnity.Editor;
using NUnit.Framework;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

public class LayeringTests
{
    [Test]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var architecture = new ArchLoader()
            .LoadAssembly(System.Reflection.Assembly.GetExecutingAssembly())
            .Build();

        Types().That().ResideInNamespace("MyGame.Domain")
            .Should().NotDependOnAny(Types().That().ResideInNamespace("MyGame.Infrastructure"))
            .Check(architecture);
    }
}
```

`Check(architecture)` (in `HamerSoft.ArchUnitUnity.Editor`) runs the rule and fails the test with ArchUnitNET's violation report upon failure. See it in action: [Tests/Editor/ArchUnitSmokeTests.cs](Tests/Editor/ArchUnitSmokeTests.cs).

### Loading assemblies by package

```csharp
using ArchUnitNET.Loader;
using HamerSoft.ArchUnitUnity.Editor;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using static HamerSoft.ArchUnitUnity.Editor.ArchUnitPackages;

var editor = Package("com.mycompany.mypackage").Editor;
var runtime = Package("com.mycompany.mypackage").Runtime;

var architecture = new ArchLoader().LoadAssemblies(editor, runtime).Build();

Types().That().ResideInAssembly(editor)
    .Should().NotDependOnAny(Types().That().ResideInAssembly(runtime))
    .Check(architecture);
```

`Package("...")` resolves a package's `Runtime`, `Editor`, `Tests`, `TestsEditor`, and `TestsRuntime` assemblies by Unity's folder convention (`null` if that part doesn't exist) — handy for asserting dependency direction between a package's own assemblies. `AsmdefAssemblyLoader.LoadAssembly(...)` does the same for any arbitrary `AssemblyDefinitionAsset` or asmdef path. See it in action: [Tests/Editor/ArchUnitSmokeTests.cs](Tests/Editor/ArchUnitSmokeTests.cs).

## CoreCLR support

Unity 7 removes the Mono-based Editor in favor of CoreCLR; Unity 6.7/6.8 offer CoreCLR as an
experimental, opt-in backend alongside Mono. `StronglyConnectedComponents.dll` is .NET Standard
and shared unchanged by both backends, but `ArchUnitNET.dll` and `System.Collections.Immutable.dll`
each need a different build per backend, so this package ships two full copies of each, gated by
`PluginImporter` define constraints — never a single shared copy patched to work both ways:

- **`ArchUnitNET.dll`** has a hard runtime dependency on `Mono.Cecil`/`Mono.Cecil.Rocks` for
  reading assembly metadata. Under Mono this has always been satisfied implicitly by Unity's own
  `com.unity.nuget.mono-cecil` package — this package never bundled Mono.Cecil itself, and still
  doesn't for the Mono variant. That package doesn't exist in Unity 7 projects, which surfaced as
  Unity flagging `ArchUnitNET.dll` itself as a "broken assembly" at code-reload time (confirmed
  against a live Unity 7 alpha Editor). Rather than vendor loose `Mono.Cecil` DLLs into the project
  for CoreCLR, `Plugins/CoreCLR/ArchUnitNET.dll` is a separate build with `Mono.Cecil` and
  `Mono.Cecil.Rocks` merged directly into it via [ILRepack](https://github.com/gluck/il-repack)
  (`/internalize`), so it carries no external `Mono.Cecil` reference at all — nothing for a missing
  Unity package to fail to resolve. Produced from the unmodified upstream `ArchUnitNET.dll` plus
  the exact `Mono.Cecil 0.11.6` / `Mono.Cecil.Rocks 0.11.6` build it already depends on:
  `ILRepack /out:ArchUnitNET.dll /internalize ArchUnitNET.dll Mono.Cecil.dll Mono.Cecil.Rocks.dll`.
  Verified functionally equivalent to the unmerged build outside Unity (same classes/rules, same
  pass/fail results) before vendoring.
- **`System.Collections.Immutable.dll`** — Mono's BCL doesn't ship this assembly but CoreCLR does;
  bundling the same copy for both risks a duplicate-assembly load failure under CoreCLR, so a
  second, newer .NET Standard 2.0 build lives at `Plugins/CoreCLR/System.Collections.Immutable.dll`.

Each pair follows the same rule: the `Plugins/` copy loads when `UNITY_7000_0_OR_NEWER` and
`ARCHUNIT_CORECLR` are both absent (Mono); the `Plugins/CoreCLR/` copy loads when either is defined.

`UNITY_7000_0_OR_NEWER` is Unity's own auto-generated version define and covers Unity 7+, where
Mono is gone. On Unity 6.7/6.8 with the experimental CoreCLR backend selected, add `ARCHUNIT_CORECLR`
to Project Settings → Player → Scripting Define Symbols yourself, since the installed Editor version
alone can't tell which backend is active there. `ScriptingBackend.IsMono` / `IsCoreClr` (in
`HamerSoft.ArchUnitUnity.Editor`) give the same signal at runtime for diagnostics.

**If you still see `ArchUnitNET.dll` flagged as a broken assembly under CoreCLR**, the first thing
to check is whether `UNITY_7000_0_OR_NEWER` is actually being generated by this Editor version (it's
alpha software, and Unity's version-define generation might not yet recognize the `7000.x` numbering)
— open Project Settings → Player → Scripting Define Symbols and confirm, or just set `ARCHUNIT_CORECLR`
manually as a direct override and retest. The `Plugins/ArchUnitNET.dll` (Mono) and
`Plugins/CoreCLR/ArchUnitNET.dll` (CoreCLR) variants must never both be enabled at once — Unity
loading two differently-built assemblies both named `ArchUnitNET` is its own source of exactly this
kind of "broken assembly" error, independent of the Mono.Cecil issue above.

**The System.Collections.Immutable split is still unverified** — if `AsmdefAssemblyLoader.LoadArchitecture`
(or `ArchLoader.Build()` directly) throws a `FileLoadException`/`TypeLoadException` mentioning
`Collections.Immutable` under CoreCLR, the likely fix is to stop bundling a plugin for that
dependency entirely and rely on CoreCLR's inbox copy — flip the
`Plugins/CoreCLR/System.Collections.Immutable.dll.meta` define constraints to never match.

## License

[MIT](License.md). Third-party licenses: [THIRDPARTY.md](THIRDPARTY.md). Changes: [CHANGELOG.md](CHANGELOG.md).
