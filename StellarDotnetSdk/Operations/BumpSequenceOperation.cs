using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
using static StellarDotnetSdk.Xdr.Operation;
using Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Bumps forward the sequence number of the source account to the given sequence number, invalidating any transaction
///     with a smaller sequence number.
///     <p>
///         See:
///         <a href="https://developers.stellar.org/docs/learn/fundamentals/transactions/list-of-operations#bump-sequence">
///             Bump
///             sequence
///         </a>
///     </p>
/// </summary>
public class BumpSequenceOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>BumpSequenceOperation</c>.
    /// </summary>
    /// <param name="bumpTo">Desired value to be bumped to.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public BumpSequenceOperation(long bumpTo, IAccountId? sourceAccount = null) : base(sourceAccount)
    {
        BumpTo = bumpTo;
    }

    /// <summary>
    ///     Desired value for the operation's source account sequence number.
    /// </summary>
    public long BumpTo { get; }

    public override OperationThreshold Threshold => OperationThreshold.LOW;

    public override OperationBody ToOperationBody()
    {
        return new OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.BUMP_SEQUENCE),
            BumpSequenceOp = new BumpSequenceOp
            {
                BumpTo = new SequenceNumber(new Int64(BumpTo)),
            },
        };
    }
}