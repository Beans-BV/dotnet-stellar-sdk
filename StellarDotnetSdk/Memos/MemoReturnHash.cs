using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Memos;

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