using System;

namespace HamerSoft.ArchUnitUnity.Editor
{
    public static class ScriptingBackend
    {
        // Mono.Runtime only exists as a type under Mono; absence implies CoreCLR.
        public static bool IsMono => Type.GetType("Mono.Runtime") != null;

        public static bool IsCoreClr => !IsMono;
    }
}
