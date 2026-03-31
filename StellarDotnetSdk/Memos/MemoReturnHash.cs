using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Memos;

/// <summary>
///     Represents a memo containing a 32-byte return hash value (XDR type <c>MEMO_RETURN</c>).
///     This is typically used to reference the hash of a transaction that the sender is refunding.
/// </summary>
public class MemoReturnHash : MemoHashAbstract
{
    public MemoReturnHash(byte[] bytes) : base(bytes)
    {
    }

    public MemoReturnHash(string hexString) : base(hexString)
    {
    }

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