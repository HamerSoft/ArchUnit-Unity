using System;
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

            return loader.Build();
        }
    }
}
