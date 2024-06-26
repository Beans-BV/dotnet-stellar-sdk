﻿using System.Collections.Generic;

namespace StellarDotnetSdk;

public static class HashCode
{
    public const int Start = 17;

    public static int Hash<T>(this int hash, T obj)
    {
        var h = EqualityComparer<T>.Default.GetHashCode(obj);
        return unchecked(hash * 31 + h);
    }
}