using System.Linq;
using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using HamerSoft.ArchUnitUnity.Editor;
using NUnit.Framework;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using static HamerSoft.ArchUnitUnity.Editor.ArchUnitPackages;
using Assembly = System.Reflection.Assembly;

namespace HamerSoft.ArchUnitUnity.Tests
{
    public class SmokeTestAllowedClass
    {
    }

    internal class SmokeTestDisallowedClass
    {
    }

    [TestFixture]
    public class ArchUnitSmokeTests
    {
        private static Architecture BuildArchitecture()
        {
            return new ArchLoader()
                .LoadAssemblies(Assembly.GetExecutingAssembly())
                .Build();
        }

        [Test]
        public void Architecture_Loads_Via_Mono()
        {
            var architecture = BuildArchitecture();

            Assert.That(architecture.Classes.Any(c => c.FullName.Contains(nameof(SmokeTestAllowedClass))), Is.True);
        }

        [Test]
        public void PassingRule_Check_DoesNotThrow()
        {
            var architecture = BuildArchitecture();

            IArchRule rule = Classes().That()
                .HaveNameEndingWith("AllowedClass")
                .Should().BePublic();

            Assert.DoesNotThrow(() => rule.Check(architecture));
        }

        [Test]
        public void FailingRule_Check_ThrowsAssertionException()
        {
            var architecture = BuildArchitecture();

            IArchRule rule = Classes().That()
                .HaveNameEndingWith("DisallowedClass")
                .Should().BePublic();

            Assert.Throws<AssertionException>(() => rule.Check(architecture));
        }

        [Test]
        public void AsmdefAssemblyLoader_ResolvesOwnAssembly()
        {
            const string asmdefPath =
                "Packages/com.hamersoft.archunit-unity/Tests/Editor/com.hamersoft.archunit-unity.test.editor.asmdef";

            var resolved = AsmdefAssemblyLoader.LoadAssembly(asmdefPath);

            Assert.That(resolved.GetName().Name, Is.EqualTo(Assembly.GetExecutingAssembly().GetName().Name));
        }

        [Test]
        public void Package_Editor_Resolves()
        {
            Assert.That(Package("com.hamersoft.archunit-unity").Editor, Is.Not.Null);
        }

        [Test]
        public void Package_TestsEditor_Resolves()
        {
            Assert.That(Package("com.hamersoft.archunit-unity").TestsEditor, Is.Not.Null);
        }

        [Test]
        public void Package_Runtime_IsNull_WhenFolderMissing()
        {
            Assert.That(Package("com.hamersoft.archunit-unity").Runtime, Is.Null);
        }

        [Test]
        public void Package_UnknownPackage_AllResolveToNull()
        {
            var package = Package("com.this.does.not.exist");

            Assert.That(package.Runtime, Is.Null);
            Assert.That(package.Editor, Is.Null);
            Assert.That(package.Tests, Is.Null);
            Assert.That(package.TestsEditor, Is.Null);
            Assert.That(package.TestsRuntime, Is.Null);
        }

        private static (Architecture Architecture, Assembly Editor, Assembly TestsEditor) BuildPackageArchitecture()
        {
            var editor = Package("com.hamersoft.archunit-unity").Editor;
            var testsEditor = Package("com.hamersoft.archunit-unity").TestsEditor;

            var architecture = new ArchLoader()
                .LoadAssemblies(editor, testsEditor)
                .Build();

            return (architecture, editor, testsEditor);
        }

        private const string EditorNamespace = "HamerSoft.ArchUnitUnity.Editor";
        private const string TestsNamespace = "HamerSoft.ArchUnitUnity.Tests";

        [Test]
        public void TestAssembly_DependsOnEditorAssembly()
        {
            var (architecture, editor, testsEditor) = BuildPackageArchitecture();

            var editorTypes = Types().That().ResideInAssembly(editor)
                .And().ResideInNamespace(EditorNamespace)
                .GetObjects(architecture).ToHashSet();
            var testTypes = Types().That().ResideInAssembly(testsEditor)
                .And().ResideInNamespace(TestsNamespace)
                .GetObjects(architecture);

            var dependsOnEditor = testTypes.Any(t => t.GetTypeDependencies(architecture).Any(editorTypes.Contains));

            Assert.That(dependsOnEditor, Is.True);
        }

        [Test]
        public void EditorAssembly_DoesNotDependOnTestAssembly()
        {
            var (architecture, editor, testsEditor) = BuildPackageArchitecture();

            IArchRule rule = Types().That().ResideInAssembly(editor)
                .And().ResideInNamespace(EditorNamespace)
                .Should().NotDependOnAny(
                    Types().That().ResideInAssembly(testsEditor)
                        .And().ResideInNamespace(TestsNamespace));

            Assert.DoesNotThrow(() => rule.Check(architecture));
        }
    }
}