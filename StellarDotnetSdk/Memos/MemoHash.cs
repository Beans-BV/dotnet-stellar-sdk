using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Memos;

/// <summary>
///     Represents a memo containing a 32-byte hash value. This is typically used to attach
///     an arbitrary hash to a transaction (XDR type <c>MEMO_HASH</c>).
/// </summary>
public class MemoHash : MemoHashAbstract
{
    /// <summary>
    ///     Initializes a new <see cref="MemoHash" /> from a 32-byte array.
    /// </summary>
    /// <param name="bytes">The 32-byte hash value.</param>
    public MemoHash(byte[] bytes) : base(bytes)
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="MemoHash" /> from a hex-encoded string.
    /// </summary>
    /// <param name="hexString">The hex-encoded hash value.</param>
    public MemoHash(string hexString) : base(hexString)
    {
    }

    /// <inheritdoc />
    public override Xdr.Memo ToXdr()
    {
        var memo = new Xdr.Memo
        {
            Discriminant = MemoType.Create(MemoType.MemoTypeEnum.MEMO_HASH),
        };

        var hash = new Hash
        {
            InnerValue = MemoBytes,
        };

        memo.Hash = hash;

        return memo;
    }
}