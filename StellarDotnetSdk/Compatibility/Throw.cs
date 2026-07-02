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
            throw new ArgumentException("Value cannot be empty.", paramName);
        }
#else
        ArgumentException.ThrowIfNullOrEmpty(value, paramName);
#endif
    }
}

