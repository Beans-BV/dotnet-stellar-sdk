using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Memos;

/// <summary>
///     Represents the absence of a memo on a transaction (XDR type <c>MEMO_NONE</c>).
/// </summary>
public class MemoNone : Memo
{
    /// <inheritdoc />
    public override Xdr.Memo ToXdr()
    {
        var memo = new Xdr.Memo
        {
            Discriminant = MemoType.Create(MemoType.MemoTypeEnum.MEMO_NONE),
        };
        return memo;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is MemoNone;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return GetType().GetHashCode();
    }
}