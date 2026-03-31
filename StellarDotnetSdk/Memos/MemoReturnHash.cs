using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Memos;

/// <summary>
///     Represents a memo containing a 32-byte return hash value (XDR type <c>MEMO_RETURN</c>).
///     This is typically used to reference the hash of a transaction that the sender is refunding.
/// </summary>
public class MemoReturnHash : MemoHashAbstract
{
    /// <summary>
    ///     Initializes a new <see cref="MemoReturnHash" /> from a 32-byte array.
    /// </summary>
    /// <param name="bytes">The 32-byte hash value referencing the returned transaction.</param>
    public MemoReturnHash(byte[] bytes) : base(bytes)
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="MemoReturnHash" /> from a hex-encoded string.
    /// </summary>
    /// <param name="hexString">The hex-encoded hash of the returned transaction.</param>
    public MemoReturnHash(string hexString) : base(hexString)
    {
    }

    /// <inheritdoc />
    public override Xdr.Memo ToXdr()
    {
        var memo = new Xdr.Memo
        {
            Discriminant = MemoType.Create(MemoType.MemoTypeEnum.MEMO_RETURN),
        };

        var hash = new Hash
        {
            InnerValue = MemoBytes,
        };

        memo.RetHash = hash;

        return memo;
    }
}