using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Memos;

/// <summary>
///     Represents a memo containing a 32-byte hash value. This is typically used to attach
///     an arbitrary hash to a transaction (XDR type <c>MEMO_HASH</c>).
/// </summary>
public class MemoHash : MemoHashAbstract
{
    public MemoHash(byte[] bytes) : base(bytes)
    {
    }

    public MemoHash(string hexString) : base(hexString)
    {
    }

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