using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Memos;

/// <summary>
///     Represents a memo containing a 64-bit unsigned integer identifier (XDR type <c>MEMO_ID</c>).
/// </summary>
public class MemoId : Memo
{
    /// <summary>
    ///     Initializes a new <see cref="MemoId" /> with the given 64-bit unsigned integer.
    /// </summary>
    /// <param name="id">The unsigned 64-bit integer value.</param>
    public MemoId(ulong id)
    {
        IdValue = id;
    }

    /// <summary>Gets the unsigned 64-bit integer value of this memo.</summary>
    public ulong IdValue { get; }

    /// <inheritdoc />
    public override Xdr.Memo ToXdr()
    {
        return new Xdr.Memo
        {
            Discriminant = MemoType.Create(MemoType.MemoTypeEnum.MEMO_ID),
            Id = new Uint64(IdValue),
        };
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is MemoId memoId && IdValue == memoId.IdValue;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Start.Hash(IdValue);
    }
}