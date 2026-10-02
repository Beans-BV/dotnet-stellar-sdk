using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using StellarDotnetSdk.Compatibility;

namespace StellarDotnetSdk;

/// <summary>
///     General-purpose utility methods for byte/hex conversion, hashing, and array manipulation
///     used throughout the Stellar SDK.
/// </summary>
public static class Util
{
    private static readonly char[] HexArray = "0123456789ABCDEF".ToCharArray();

    // Throws on malformed input instead of substituting U+FFFD, so distinct byte strings never
    // decode to the same text.
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    /// <summary>
    ///     Converts a byte array to its uppercase hexadecimal string representation.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>The hexadecimal string representation.</returns>
    public static string BytesToHex(byte[] bytes)
    {
        var hexChars = new char[bytes.Length * 2];
        for (var j = 0; j < bytes.Length; j++)
        {
            var v = bytes[j] & 0xFF;
            hexChars[j * 2] = HexArray[(uint)v >> 4];
            hexChars[j * 2 + 1] = HexArray[v & 0x0F];
        }

        return new string(hexChars);
    }

    /// <summary>
    ///     Converts a hexadecimal string to its byte array representation.
    /// </summary>
    /// <param name="s">The hexadecimal string to convert.</param>
    /// <returns>The decoded byte array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="s" /> is null.</exception>
    /// <exception cref="FormatException">
    ///     Thrown when <paramref name="s" /> has an odd length or contains a non-hexadecimal character —
    ///     the same exception type <c>Convert.FromHexString</c> (.NET 5 and later) throws for such input.
    /// </exception>
    public static byte[] HexToBytes(string s)
    {
        Throw.IfNull(s, nameof(s));
        if (s.Length % 2 != 0)
        {
            throw new FormatException("The input is not a valid hex string as its length is not a multiple of 2.");
        }

        var len = s.Length;
        var data = new byte[len / 2];
        for (var i = 0; i < len; i += 2)
        {
            data[i / 2] = (byte)((Convert.ToByte(s[i].ToString(), 16) << 4)
                                 + Convert.ToByte(s[i + 1].ToString(), 16));
        }
        return data;
    }

    /// <summary>
    ///     Returns SHA-256 hash of data.
    /// </summary>
    /// <param name="data">data</param>
    /// <returns>Sha-256 Hash</returns>
    public static byte[] Hash(byte[] data)
    {
#if NET8_0_OR_GREATER
        return SHA256.HashData(data);
#else
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(data);
#endif
    }

    /// <summary>
    ///     Pads byte array to length with zeros.
    /// </summary>
    /// <param name="bytes">byte array</param>
    /// <param name="length">length</param>
    /// <returns>zero padded byte array</returns>
    public static byte[] PaddedByteArray(byte[] bytes, int length)
    {
        var finalBytes = new byte[length];
        Fill(finalBytes, (byte)0);
        Array.Copy(bytes, 0, finalBytes, 0, bytes.Length);

        return finalBytes;
    }

    /// <summary>
    ///     Pads string to length with zeros.
    /// </summary>
    /// <param name="source">string to pad</param>
    /// <param name="length">length</param>
    /// <returns>zero padded byte array</returns>
    public static byte[] PaddedByteArray(string source, int length)
    {
        return PaddedByteArray(Encoding.Default.GetBytes(source), length);
    }

    /// <summary>
    ///     Remove zeros from the end of byte array.
    /// </summary>
    /// <param name="bytes">byte array</param>
    /// <returns>string with padded zeros removed</returns>
    public static string PaddedByteArrayToString(byte[] bytes)
    {
        return Encoding.Default.GetString(bytes).Split('\0')[0];
    }

    /// <summary>
    ///     Determines whether two byte arrays are identical (same length and content).
    /// </summary>
    /// <param name="a1">The first byte array.</param>
    /// <param name="a2">The second byte array.</param>
    /// <returns><c>true</c> if the arrays are identical; otherwise, <c>false</c>.</returns>
    public static bool IsIdentical(this byte[] a1, byte[] a2)
    {
        if (a1.Length != a2.Length)
        {
            return false;
        }

        return !a1.Where((t, i) => t != a2[i]).Any();
    }

    private static void Fill<T>(this T[] arr, T value)
    {
        for (var i = 0; i < arr.Length; i++)
        {
            arr[i] = value;
        }
    }

    /// <summary>
    ///     Converts a 64-bit unsigned integer to a big-endian byte array.
    /// </summary>
    /// <param name="value">The unsigned 64-bit integer to convert.</param>
    /// <returns>An 8-byte big-endian byte array.</returns>
    public static byte[] ToByteArray(ulong value)
    {
        var result = new byte[8];
        for (var i = 7; i >= 0; i--)
        {
            result[i] = (byte)(value & 0xffL);
            value >>= 8;
        }

        return result;
    }

    /// <summary>
    ///     Decodes <paramref name="bytes" /> as strict UTF-8. Unlike <see cref="Encoding.UTF8" />, this never
    ///     replaces malformed input with U+FFFD, so it fails rather than returning text that no longer identifies
    ///     the original bytes.
    /// </summary>
    /// <param name="bytes">The bytes to decode.</param>
    /// <param name="value">The decoded text, or <see langword="null" /> when the bytes are not valid UTF-8.</param>
    /// <returns><see langword="true" /> when the bytes are valid UTF-8.</returns>
    internal static bool TryDecodeUtf8(byte[] bytes, out string? value)
    {
#if NET8_0_OR_GREATER
        // Rejects the same malformed input as StrictUtf8 without throwing, so a binary tag costs no exception.
        if (!System.Text.Unicode.Utf8.IsValid(bytes))
        {
            value = null;
            return false;
        }
#endif
        try
        {
            value = StrictUtf8.GetString(bytes);
            return true;
        }
        catch (DecoderFallbackException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    ///     Encodes <paramref name="value" /> as strict UTF-8. Unlike <see cref="Encoding.UTF8" />, this never replaces
    ///     an unpaired surrogate with U+FFFD, so two different strings never encode to the same bytes.
    /// </summary>
    /// <param name="value">The text to encode.</param>
    /// <param name="paramName">The name of the caller's parameter, reported when the text is rejected.</param>
    /// <returns>The UTF-8 bytes of <paramref name="value" />.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value" /> contains an unpaired surrogate.</exception>
    internal static byte[] EncodeUtf8Strict(string value, string paramName)
    {
        try
        {
            return StrictUtf8.GetBytes(value);
        }
        catch (EncoderFallbackException ex)
        {
            throw new ArgumentException(
                "The text contains an unpaired surrogate, which has no UTF-8 encoding; pass the bytes instead.",
                paramName, ex);
        }
    }

    /// <summary>
    ///     Computes an FNV-1a hash of the given byte array.
    /// </summary>
    /// <param name="data">The byte array to hash.</param>
    /// <returns>A 32-bit hash code.</returns>
    public static int ComputeByteArrayHash(params byte[] data)
    {
        unchecked
        {
            const int p = 16777619;
            var hash = data.Aggregate((int)2166136261, (current, t) => (current ^ t) * p);

            hash += hash << 13;
            hash ^= hash >> 7;
            hash += hash << 3;
            hash ^= hash >> 17;
            hash += hash << 5;
            return hash;
        }
    }
}