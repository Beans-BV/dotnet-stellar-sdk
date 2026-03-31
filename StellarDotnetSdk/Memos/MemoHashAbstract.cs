using System;
using System.Linq;
using StellarDotnetSdk.Exceptions;

namespace StellarDotnetSdk.Memos;

/// <summary>
///     Abstract base class for memo types that contain a 32-byte hash value (<see cref="MemoHash" />
///     and <see cref="MemoReturnHash" />).
/// </summary>
public abstract class MemoHashAbstract : Memo
{
    /// <summary>
    ///     Initializes a new instance from a byte array. Arrays shorter than 32 bytes are zero-padded.
    /// </summary>
    /// <param name="bytes">The hash bytes (max 32 bytes).</param>
    public MemoHashAbstract(byte[] bytes)
    {
        if (bytes.Length < 32)
        {
            bytes = Util.PaddedByteArray(bytes, 32);
        }
        else if (bytes.Length > 32)
        {
            throw new MemoTooLongException("MEMO_HASH can contain 32 bytes at max.");
        }

        MemoBytes = bytes;
    }

    /// <summary>
    ///     Initializes a new instance from a hex-encoded string.
    /// </summary>
    /// <param name="hexString">The hex-encoded hash value.</param>
    public MemoHashAbstract(string hexString) : this(Util.HexToBytes(hexString))
    {
    }

    /// <summary>Gets the raw 32-byte hash value.</summary>
    public byte[] MemoBytes { get; }

    /// <summary>
    ///     <p>Returns hex representation of bytes contained in this memo.</p>
    ///     <p>Example:</p>
    ///     <code>
    ///    MemoHash memo = new MemoHash("4142434445");
    ///    memo.getHexValue(); // 4142434445000000000000000000000000000000000000000000000000000000
    ///    memo.getTrimmedHexValue(); // 4142434445
    ///  </code>
    /// </summary>
    public string GetHexValue()
    {
        return Util.BytesToHex(MemoBytes).ToLower();
    }

    /// <summary>
    ///     <p>Returns hex representation of bytes contained in this memo until null byte (0x00) is found.</p>
    ///     <p>Example:</p>
    ///     <code>
    ///    MemoHash memo = new MemoHash("4142434445");
    ///    memo.getHexValue(); // 4142434445000000000000000000000000000000000000000000000000000000
    ///    memo.getTrimmedHexValue(); // 4142434445
    ///  </code>
    /// </summary>
    public string GetTrimmedHexValue()
    {
        return GetHexValue().Split(new[] { "00" }, StringSplitOptions.None)[0].ToLower();
    }

    /// <inheritdoc />
    public abstract override Xdr.Memo ToXdr();

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is MemoHashAbstract that && MemoBytes.SequenceEqual(that.MemoBytes);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Start.Hash(Util.ComputeByteArrayHash(MemoBytes));
    }
}