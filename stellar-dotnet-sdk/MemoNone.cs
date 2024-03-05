using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class MemoNone : Memo
{
    public override xdr.Memo ToXdr()
    {
        var memo = new xdr.Memo
        {
            Discriminant = MemoType.Create(MemoType.MemoTypeEnum.MEMO_NONE)
        };
        return memo;
    }

    public override bool Equals(object? obj)
    {
        return obj is MemoNone;
    }

    public override int GetHashCode()
    {
        return GetType().GetHashCode();
    }
}