using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Terminates the current is-sponsoring-future-reserves relationship in which the source account is sponsored.
///     <p>Use <see cref="Builder" /> to create a new <c>EndSponsoringFutureReservesOperation</c>.</p>
///     See:
///     <see href="https://developers.stellar.org/docs/encyclopedia/sponsored-reserves#begin-and-end-sponsorships">
///         Begin and end sponsorships
///     </see>
/// </summary>
public class EndSponsoringFutureReservesOperation : Operation
{
    private EndSponsoringFutureReservesOperation()
    {
    }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.END_SPONSORING_FUTURE_RESERVES)
        };
    }

    /// <summary>
    ///     Builder for <c>EndSponsoringFutureReservesOperation</c>.
    /// </summary>
    public class Builder
    {
        private KeyPair? _sourceAccount;

        public Builder()
        {
        }

        /// <summary>
        ///     Constructs a new <c>EndSponsoringFutureReservesOperation</c> builder.
        /// </summary>
        /// <param name="accountId">The account which initiated the sponsorship.</param>
        public Builder(KeyPair account)
        {
            _sourceAccount = account;
        }

        /// <summary>
        ///     Constructs a new <c>EndSponsoringFutureReservesOperation</c> builder.
        /// </summary>
        /// <param name="accountId">Ed25519 public key of the account which initiated the sponsorship.</param>
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
        ///     Builds an operation.
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