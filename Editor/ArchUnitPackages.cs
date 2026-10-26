using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace HamerSoft.ArchUnitUnity.Editor
{
    public readonly struct PackageAssemblies
    {
        private readonly PackageInfo _package;

        internal PackageAssemblies(string packageName)
        {
            _package = PackageInfo.GetAllRegisteredPackages().FirstOrDefault(p => p.name == packageName);
        }

        public Assembly Runtime => Resolve("Runtime");
        public Assembly Editor => Resolve("Editor");
        public Assembly Tests => Resolve("Tests");
        public Assembly TestsEditor => Resolve("Tests/Editor");
        public Assembly TestsRuntime => Resolve("Tests/Runtime");

        private Assembly Resolve(string relativeFolder)
        {
            if (_package == null)
                return null;

            var folder = $"{_package.assetPath}/{relativeFolder}";
            if (!AssetDatabase.IsValidFolder(folder))
                return null;

            var asmdefPath = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(p => Path.GetDirectoryName(p)?.Replace('\\', '/') == folder);

            return asmdefPath == null ? null : AsmdefAssemblyLoader.LoadAssembly(asmdefPath);
        }
    }

    public static class ArchUnitPackages
    {
        public static PackageAssemblies Package(string packageName) => new PackageAssemblies(packageName);
    }
}
