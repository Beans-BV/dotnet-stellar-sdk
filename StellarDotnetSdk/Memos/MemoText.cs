using System;
using System.Text;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Memos;

/// <summary>
///     Represents a memo containing a UTF-8 text string of up to 28 bytes (XDR type <c>MEMO_TEXT</c>).
/// </summary>
public class MemoText : Memo
{
    /// <summary>
    ///     Initializes a new <see cref="MemoText" /> with the given UTF-8 string (max 28 bytes).
    /// </summary>
    /// <param name="text">The text content (must be at most 28 bytes when UTF-8 encoded).</param>
    public MemoText(string text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text), "text cannot be null");
        }

        var bytes = Encoding.UTF8.GetBytes(text);
        if (bytes.Length > 28)
        {
            throw new MemoTooLongException("text must be <= 28 bytes. length=" + bytes.Length);
        }

        MemoBytesValue = bytes;
    }

    /// <summary>
    ///     Initializes a new <see cref="MemoText" /> with the given raw bytes (max 28 bytes).
    /// </summary>
    /// <param name="text">The raw byte content (max 28 bytes).</param>
    public MemoText(byte[] text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text), "text cannot be null");
        }
        if (text.Length > 28)
        {
            throw new MemoTooLongException("text must be <= 28 bytes. length=" + text.Length);
        }

        MemoBytesValue = text;
    }

    /// <summary>
    ///     Gets the memo content decoded as a UTF-8 string.
    /// </summary>
    public string MemoTextValue => Encoding.UTF8.GetString(MemoBytesValue);

    /// <summary>Gets the raw byte content of this text memo.</summary>
    public byte[] MemoBytesValue { get; }

    /// <inheritdoc />
    public override Xdr.Memo ToXdr()
    {
        return new Xdr.Memo
        {
            Discriminant = MemoType.Create(MemoType.MemoTypeEnum.MEMO_TEXT),
            Text = MemoTextValue,
        };
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is MemoText memoText && Equals(MemoTextValue, memoText.MemoTextValue);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Start
            .Hash(MemoTextValue);
    }
}