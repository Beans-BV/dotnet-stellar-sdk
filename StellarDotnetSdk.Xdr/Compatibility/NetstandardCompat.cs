using System;
using System.Collections.Generic;
using System.IO;

namespace StellarDotnetSdk.Xdr.Compatibility;

internal static class NetstandardCompat
{
    public static void AddRangeCompat(this List<byte> bytes, ReadOnlySpan<byte> buffer)
    {
#if NET8_0_OR_GREATER
        bytes.AddRange(buffer);
#else
        // List<T>.AddRange(ReadOnlySpan<T>) is not available on netstandard2.1.
        bytes.AddRange(buffer.ToArray());
#endif
    }

    public static void ReadExactlyCompat(this Stream stream, Span<byte> buffer)
    {
#if NET8_0_OR_GREATER
        stream.ReadExactly(buffer);
#else
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            var read = stream.Read(buffer.Slice(totalRead));
            if (read <= 0)
            {
                throw new EndOfStreamException(
                    $"Unable to read required number of bytes. Expected {buffer.Length}, got {totalRead}.");
            }

            totalRead += read;
        }
#endif
    }

    public static void ReadExactlyCompat(this Stream stream, byte[] buffer)
    {
        stream.ReadExactlyCompat(buffer.AsSpan());
    }
}

