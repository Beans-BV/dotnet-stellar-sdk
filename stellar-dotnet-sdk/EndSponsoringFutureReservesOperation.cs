using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <c>xdr.EndSponsoringFutureReservesOp</c>.
///     Use <see cref="Builder" /> to create a new <c>EndSponsoringFutureReservesOperation</c>.
///     See:
///     <see href="https://developers.stellar.org/docs/encyclopedia/sponsored-reserves#begin-and-end-sponsorships">
///         Begin
///         and end sponsorships
///     </see>
/// </summary>
public class EndSponsoringFutureReservesOperation : Operation
{
    private EndSponsoringFutureReservesOperation()
    {
    }

    public override xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.END_SPONSORING_FUTURE_RESERVES)
        };
        return body;
    }

    /// <summary>
    ///     Builds EndSponsoringFutureReserves operation.
    /// </summary>
    /// <see cref="EndSponsoringFutureReservesOperation" />
    public class Builder
    {
        private KeyPair? _sourceAccount;

        public Builder()
        {
        }
        
        public Builder(KeyPair account)
        {
            _sourceAccount = account;
        }
        
        public Builder(string accountId) : this(KeyPair.FromAccountId(accountId))
        {
        }
        
        /// <summary>
        ///     Sets the source account for this operation.
        /// </summary>
        /// <param name="account">The operation's source account.</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(KeyPair account)
        {
            _sourceAccount = account;
            return this;
        }

        /// <summary>
        ///     Builds an operation
        /// </summary>
        public EndSponsoringFutureReservesOperation Build()
        {
            var operation = new EndSponsoringFutureReservesOperation();
            if (_sourceAccount != null)
                operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}