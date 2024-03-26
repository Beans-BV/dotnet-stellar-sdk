using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class MemoId : Memo
{
    public MemoId(ulong id)
    {
        IdValue = id;
    }

    public ulong IdValue { get; }

    public override xdr.Memo ToXdr()
    {
        var memo = new xdr.Memo
        {
            Discriminant = MemoType.Create(MemoType.MemoTypeEnum.MEMO_ID)
        };
        var idXdr = new Uint64
        {
            InnerValue = IdValue
        };
        memo.Id = idXdr;
        return memo;
    }

    public override bool Equals(object? obj)
    {
        return obj is MemoId memoId && IdValue == memoId.IdValue;
    }

    public override int GetHashCode()
    {
        return HashCode.Start.Hash(IdValue);
    }
}