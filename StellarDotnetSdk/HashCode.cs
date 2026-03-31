using System.Collections.Generic;

namespace StellarDotnetSdk;

/// <summary>
///     Provides fluent hash code generation utilities using the FNV-style chained hashing pattern.
/// </summary>
public static class HashCode
{
    /// <summary>The initial hash value used to begin a hash chain.</summary>
    public const int Start = 17;

    /// <summary>
    ///     Combines the current hash value with the hash of the given object.
    /// </summary>
    /// <typeparam name="T">The type of the object to hash.</typeparam>
    /// <param name="hash">The current hash value in the chain.</param>
    /// <param name="obj">The object whose hash code to incorporate.</param>
    /// <returns>A new combined hash code.</returns>
    public static int Hash<T>(this int hash, T obj)
    {
        var h = EqualityComparer<T>.Default.GetHashCode(obj);
        return unchecked(hash * 31 + h);
    }
}