using System;
using System.Text;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Memos;

public class MemoText : Memo
{
    public MemoText(string text)
    {
        if (text is null)
            throw new ArgumentNullException(nameof(text), "text cannot be null");

        var bytes = Encoding.UTF8.GetBytes(text);
        if (bytes.Length > 28)
            throw new MemoTooLongException("text must be <= 28 bytes. length=" + bytes.Length);

        MemoBytesValue = bytes;
    }

    public MemoText(byte[] text)
    {
        if (text is null)
            throw new ArgumentNullException(nameof(text), "text cannot be null");
        if (text.Length > 28)
            throw new MemoTooLongException("text must be <= 28 bytes. length=" + text.Length);

        MemoBytesValue = text;
    }

    public string MemoTextValue => Encoding.UTF8.GetString(MemoBytesValue);
    public byte[] MemoBytesValue { get; }

    public override Xdr.Memo ToXdr()
    {
        return new Xdr.Memo
        {
            Discriminant = MemoType.Create(MemoType.MemoTypeEnum.MEMO_TEXT),
            Text = MemoTextValue
        };
    }

    public override bool Equals(object? obj)
    {
        return obj is MemoText memoText && Equals(MemoTextValue, memoText.MemoTextValue);
    }

    public override int GetHashCode()
    {
        return HashCode.Start
            .Hash(MemoTextValue);
    }
}