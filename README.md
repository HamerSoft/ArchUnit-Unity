# ArchUnit-Unity

[ArchUnitNET](https://github.com/TNG/ArchUnitNET) wrapped for Unity's test framework, so you can write architecture tests (layering rules, dependency constraints, naming conventions, ...) as regular EditMode tests. Works on both the Mono and CoreCLR scripting backends.

## Requirements

- Unity 2021.2+ (.NET Standard 2.1 API compatibility)
- Mono or CoreCLR scripting backend — both are supported transparently, nothing to configure (see [CoreCLR support](#coreclr-support) below for the mechanics)

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

This package works on both the Mono and CoreCLR scripting backends — nothing to configure, no
source-level differences. `ArchUnitNET.dll` and `StronglyConnectedComponents.dll` are .NET Standard
and shared unchanged by both backends — same unmodified upstream build either way, same as a
regular (non-Unity) ArchUnitNET integration.

What differs is two of ArchUnitNET's own runtime dependencies, each bundled in a backend-specific
`Plugins/` vs `Plugins/CoreCLR/` variant, gated by `PluginImporter` define constraints:

- **`Mono.Cecil.dll` / `Mono.Cecil.Rocks.dll`** — ArchUnitNET's hard dependency for reading
  assembly metadata (confirmed straight from `ArchUnitNET.dll`'s own AssemblyRef table: exact
  version `0.11.6`, still required by the current upstream release). Under Mono this is satisfied
  implicitly by Unity's own `com.unity.nuget.mono-cecil` package — nothing bundled here for that
  path. That package doesn't exist in Unity 7 projects, which surfaced as Unity flagging
  `ArchUnitNET.dll` itself as a "broken assembly" at code-reload time (confirmed against a live
  Unity 7 alpha Editor). `Plugins/CoreCLR/Mono.Cecil.dll` and `Plugins/CoreCLR/Mono.Cecil.Rocks.dll`
  — the same `0.11.6` build ArchUnitNET already depends on, vendored as plain, unmodified files —
  cover that gap under CoreCLR.
- **`System.Collections.Immutable.dll`** — Mono's BCL doesn't ship this assembly but CoreCLR does;
  bundling the same copy for both risks a duplicate-assembly load failure under CoreCLR, so a
  second, newer .NET Standard 2.0 build lives at `Plugins/CoreCLR/System.Collections.Immutable.dll`.

`System.Collections.Immutable.dll` is a true pair: the `Plugins/` copy loads when
`UNITY_7000_0_OR_NEWER` is absent (Mono), the `Plugins/CoreCLR/` copy loads when it's defined.
`Mono.Cecil.dll`/`Mono.Cecil.Rocks.dll` only exist under `Plugins/CoreCLR/`, gated the same way —
there's nothing to toggle on the Mono side since that path never bundles them at all.
`UNITY_7000_0_OR_NEWER` is Unity's own auto-generated version define — confirmed actually generated
by a live Unity 7000.0.0a12 alpha Editor, and this setup is confirmed working against both a live
Unity 6 (Mono) and Unity 7 alpha (CoreCLR) Editor, including a standalone functional check (same
classes discovered, same rule pass/fail results) outside Unity before vendoring. The classic
`Type.GetType("Mono.Runtime") != null` trick (that type only exists under Mono) backs the "Mono"
vs "CoreCLR" naming used in diagnostics — see `AsmdefAssemblyLoader.LoadArchitecture`'s
dependency-load error message and the smoke tests.

**Known limitation:** Unity 6.7/6.8 offer CoreCLR as an experimental, opt-in backend alongside
Mono, selected per-project in Project Settings → Player → Scripting Backend — the Editor version
number alone can't tell which backend is active there, so this package doesn't yet pick the right
`Mono.Cecil`/`System.Collections.Immutable` variant on those versions regardless of what's
selected; it always behaves as if Mono is active since `UNITY_7000_0_OR_NEWER` is false. Not
addressed for now since it's untested and only matters for that experimental, non-default backend —
if you need it, add a manual override define (e.g. `ARCHUNIT_CORECLR`) as an `OR` clause on
`Plugins/CoreCLR/Mono.Cecil.dll.meta`, `Plugins/CoreCLR/Mono.Cecil.Rocks.dll.meta`, and
`Plugins/CoreCLR/System.Collections.Immutable.dll.meta`'s define constraints, matching that same
symbol as an `AND NOT` on `Plugins/System.Collections.Immutable.dll.meta` (the Mono side has no
`Mono.Cecil` variant of its own to exclude — it never bundles one), and actually test the 6.7/6.8
CoreCLR path before relying on it.

## License

[MIT](License.md). Third-party licenses: [THIRDPARTY.md](THIRDPARTY.md). Changes: [CHANGELOG.md](CHANGELOG.md).
