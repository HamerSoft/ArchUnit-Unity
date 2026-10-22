# ArchUnit-Unity

[ArchUnitNET](https://github.com/TNG/ArchUnitNET) wrapped for Unity's test framework, so you can write architecture tests (layering rules, dependency constraints, naming conventions, ...) as regular EditMode tests.

## Requirements

- Unity 2021.2+ (.NET Standard 2.1 API compatibility)

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

## License

[MIT](License.md). Third-party licenses: [THIRDPARTY.md](THIRDPARTY.md).
