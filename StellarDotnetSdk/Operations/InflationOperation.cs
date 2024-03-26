using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Use <see cref="Builder" /> to create a new InflationOperation.
///     See also:
///     <see href="https://developers.stellar.org/docs/learn/encyclopedia/inflation">Inflation</see>
/// </summary>
[Obsolete("This operation is deprecated as of Protocol 17- prefer SetTrustlineFlags instead.")]
public class InflationOperation : Operation
{
    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.INFLATION)
        };
        return body;
    }

    public class Builder
    {
        private IAccountId? _mSourceAccount;

        /// <summary>
        ///     Sets the source account for this operation.
        /// </summary>
        /// <param name="sourceAccount">The operation's source account.</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(IAccountId sourceAccount)
        {
            _mSourceAccount = sourceAccount ??
                              throw new ArgumentNullException(nameof(sourceAccount), "sourceAccount cannot be null.");
            return this;
        }

        /// <summary>
        ///     Builds an operation
        /// </summary>
        public InflationOperation Build()
        {
            var operation = new InflationOperation();
            if (_mSourceAccount != null)
                operation.SourceAccount = _mSourceAccount;
            return operation;
        }
    }
}