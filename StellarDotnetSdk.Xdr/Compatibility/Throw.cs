using System;

namespace StellarDotnetSdk.Xdr.Compatibility;

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
}
