using System;

namespace StellarDotnetSdk.Compatibility;

internal static class Throw
{
    public static void IfNull(object? value, string paramName)
    {
#if NETSTANDARD2_1
        if (value == null)
        {
            throw new ArgumentNullException(paramName);
        }
#else
        ArgumentNullException.ThrowIfNull(value, paramName);
#endif
    }

    public static void IfNullOrEmpty(string? value, string paramName)
    {
#if NETSTANDARD2_1
        if (value == null)
        {
            throw new ArgumentNullException(paramName);
        }

        if (value.Length == 0)
        {
            // Same text as ArgumentException.ThrowIfNullOrEmpty on net8.0+, so exception messages
            // do not diverge between target frameworks.
            throw new ArgumentException("The value cannot be an empty string.", paramName);
        }
#else
        ArgumentException.ThrowIfNullOrEmpty(value, paramName);
#endif
    }
}