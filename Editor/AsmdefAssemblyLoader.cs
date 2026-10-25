using System;
using System.IO;
using System.Linq;
using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace HamerSoft.ArchUnitUnity.Editor
{
    public static class AsmdefAssemblyLoader
    {
        private static readonly string[] ArchUnitNetDependencyNames =
            { "Mono.Cecil", "Collections.Immutable" };

        [Serializable]
        private class AsmdefName
        {
            public string name;
        }

        public static Assembly LoadAssembly(AssemblyDefinitionAsset asmdefAsset)
        {
            if (asmdefAsset == null)
                throw new ArgumentNullException(nameof(asmdefAsset));

            var assemblyName = JsonUtility.FromJson<AsmdefName>(asmdefAsset.text).name;
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            if (assembly == null)
                throw new InvalidOperationException(
                    $"Assembly '{assemblyName}' from asmdef '{AssetDatabase.GetAssetPath(asmdefAsset)}' is not currently loaded in this domain. " +
                    "Make sure the asmdef compiles and is included for the Editor platform.");

            return assembly;
        }

        public static Assembly LoadAssembly(string asmdefPath)
        {
            if (string.IsNullOrEmpty(asmdefPath))
                throw new ArgumentException("Asmdef path must not be null or empty.", nameof(asmdefPath));

            var asmdefAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(asmdefPath);
            if (asmdefAsset == null)
                throw new ArgumentException($"No AssemblyDefinitionAsset found at '{asmdefPath}'.",
                    nameof(asmdefPath));

            return LoadAssembly(asmdefAsset);
        }

        public static Architecture LoadArchitecture(params AssemblyDefinitionAsset[] asmdefAssets)
        {
            if (asmdefAssets == null || asmdefAssets.Length == 0)
                throw new ArgumentException("At least one asmdef must be provided.", nameof(asmdefAssets));

            var loader = new ArchLoader();
            foreach (var asmdefAsset in asmdefAssets)
                loader.LoadAssembly(LoadAssembly(asmdefAsset));

            try
            {
                return loader.Build();
            }
            catch (Exception e) when (e is FileLoadException or TypeLoadException &&
                                       ArchUnitNetDependencyNames.Any(e.Message.Contains))
            {
                throw new InvalidOperationException(
                    "ArchUnitNET failed to load one of its bundled dependencies " +
                    $"(Mono.Cecil, Mono.Cecil.Rocks, System.Collections.Immutable) under the " +
                    $"{(ScriptingBackend.IsCoreClr ? "CoreCLR" : "Mono")} scripting backend. Check that the " +
                    "right variant under Plugins/ vs Plugins/CoreCLR/ is enabled for this backend " +
                    "(their .dll.meta define constraints gate on UNITY_7000_0_OR_NEWER) " +
                    "and that nothing else in the project already loaded a conflicting copy of the same assembly.",
                    e);
            }
        }
    }
}
