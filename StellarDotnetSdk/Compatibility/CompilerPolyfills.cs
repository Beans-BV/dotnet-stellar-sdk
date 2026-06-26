// Compiler polyfills required for netstandard2.1 builds.
// These types are normally provided by newer runtimes / reference assemblies.

#if NETSTANDARD2_1
namespace System.Runtime.CompilerServices
{
    // Support for init-only setters (C# 9).
    internal static class IsExternalInit
    {
    }

    // Support for required members (C# 11).
    [System.AttributeUsage(
        System.AttributeTargets.Class |
        System.AttributeTargets.Struct |
        System.AttributeTargets.Field |
        System.AttributeTargets.Property,
        Inherited = false,
        AllowMultiple = false)]
    internal sealed class RequiredMemberAttribute : System.Attribute
    {
    }

    // Used by the compiler to annotate features such as required members.
    [System.AttributeUsage(
        System.AttributeTargets.Class |
        System.AttributeTargets.Struct |
        System.AttributeTargets.Field |
        System.AttributeTargets.Property,
        Inherited = false,
        AllowMultiple = true)]
    internal sealed class CompilerFeatureRequiredAttribute : System.Attribute
    {
        public CompilerFeatureRequiredAttribute(string featureName)
        {
            FeatureName = featureName;
        }

        public string FeatureName { get; }

        public bool IsOptional { get; set; }
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    // Support for required members analysis (C# 11).
    [System.AttributeUsage(System.AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
    internal sealed class SetsRequiredMembersAttribute : System.Attribute
    {
    }
}
#endif
