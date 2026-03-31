using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Memos;

/// <summary>
///     Represents a memo containing a 64-bit unsigned integer identifier (XDR type <c>MEMO_ID</c>).
/// </summary>
public class MemoId : Memo
{
    public MemoId(ulong id)
    {
        IdValue = id;
    }

    public ulong IdValue { get; }

    public override Xdr.Memo ToXdr()
    {
        return new Xdr.Memo
        {
            Discriminant = MemoType.Create(MemoType.MemoTypeEnum.MEMO_ID),
            Id = new Uint64(IdValue),
        };
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