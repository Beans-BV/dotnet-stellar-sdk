using System;
using stellar_dotnet_sdk.xdr;
using static stellar_dotnet_sdk.xdr.Operation;
using Int64 = stellar_dotnet_sdk.xdr.Int64;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <see cref="BumpSequenceOp" />.
///     Use <see cref="Builder" /> to create a new BumpSequenceOperation.
///     See also:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#bump-sequence">Bump Sequence</a>
/// </summary>
public class BumpSequenceOperation : Operation
{
    public BumpSequenceOperation(long bumpTo)
    {
        BumpTo = bumpTo;
    }

    public long BumpTo { get; }

    public override OperationThreshold Threshold => OperationThreshold.LOW;

    public override OperationBody ToOperationBody()
    {
        var body = new OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.BUMP_SEQUENCE),
            BumpSequenceOp = new BumpSequenceOp
            {
                BumpTo = new SequenceNumber
                {
                    InnerValue = new Int64
                    {
                        InnerValue = BumpTo,
                    },
                },
            }
        };
        return body;
    }

    public class Builder
    {
        private KeyPair? _sourceAccount;

        public Builder(BumpSequenceOp op)
        {
            BumpTo = op.BumpTo.InnerValue.InnerValue;
        }

        public Builder(long bumpTo)
        {
            BumpTo = bumpTo;
        }

        public long BumpTo { get; }

        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _sourceAccount = sourceAccount ??
                             throw new ArgumentNullException(nameof(sourceAccount), "sourceAccount cannot be null");
            return this;
        }

        public BumpSequenceOperation Build()
        {
            var operation = new BumpSequenceOperation(BumpTo);
            if (_sourceAccount != null) operation.SourceAccount = _sourceAccount;

            return operation;
        }
    }
}