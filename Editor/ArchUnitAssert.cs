using System.Linq;
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Extensions;
using NUnit.Framework;

namespace HamerSoft.ArchUnitUnity.Editor
{
    public static class ArchUnitAssert
    {
        public static void Check(this IArchRule rule, Architecture architecture)
        {
            var violations = rule.Evaluate(architecture).Where(r => !r.Passed).ToList();
            Assert.IsEmpty(violations, violations.ToErrorMessage());
        }

        public static void CheckAll(this Architecture architecture, params IArchRule[] rules)
        {
            foreach (var rule in rules)
                rule.Check(architecture);
        }
    }
}
