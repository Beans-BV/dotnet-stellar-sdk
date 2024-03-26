using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
using static StellarDotnetSdk.Xdr.Operation;
using xdr_Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Bumps forward the sequence number of the source account to the given sequence number, invalidating any transaction
///     with a smaller sequence number.
///     Use <see cref="Builder" /> to create a new <c>BumpSequenceOperation</c>.
///     <p>
///         See:
///         <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#bump-sequence">Bump sequence</a>
///     </p>
/// </summary>
public class BumpSequenceOperation : Operation
{
    private BumpSequenceOperation(long bumpTo)
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
                BumpTo = new SequenceNumber(new xdr_Int64(BumpTo))
            }
        };
    }

    /// <summary>
    ///     Builder for <c>BumpSequenceOperation</c>.
    /// </summary>
    public class Builder
    {
        private KeyPair? _sourceAccount;

        /// <summary>
        ///     Constructs a new <c>BumpSequenceOperation</c> builder.
        /// </summary>
        /// <param name="bumpTo">Desired value to be bumped to.</param>
        public Builder(long bumpTo)
        {
            BumpTo = bumpTo;
        }

        private long BumpTo { get; }

        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _sourceAccount = sourceAccount ??
                             throw new ArgumentNullException(nameof(sourceAccount), "sourceAccount cannot be null");
            return this;
        }

        /// <summary>
        ///     Builds an operation.
        /// </summary>
        public BumpSequenceOperation Build()
        {
            var operation = new BumpSequenceOperation(BumpTo);
            if (_sourceAccount != null) operation.SourceAccount = _sourceAccount;

            return operation;
        }
    }
}