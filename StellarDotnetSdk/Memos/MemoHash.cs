using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Memos;

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