using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     <p>
///         Allows an account to pay the base reserves for another account; sponsoring account establishes the
///         is-sponsoring-future-reserves relationship.
///     </p>
///     <p>There must also be an end sponsoring future reserves operation in the same transaction.</p>
///     Use <see cref="Builder" /> to create a new <c>BeginSponsoringFutureReservesOperation</c>.
///     <p>
///         See:
///         <a
///             href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#begin-sponsoring-future-reserves">
///             Begin sponsoring future reserves
///         </a>
///     </p>
/// </summary>
public class BeginSponsoringFutureReservesOperation : Operation
{
    private BeginSponsoringFutureReservesOperation(KeyPair sponsoredId)
    {
        SponsoredId = sponsoredId ?? throw new ArgumentNullException(nameof(sponsoredId));
    }

    /// <summary>
    ///     The sponsored account.
    /// </summary>
    public KeyPair SponsoredId { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant =
                OperationType.Create(OperationType.OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES),
            BeginSponsoringFutureReservesOp = new BeginSponsoringFutureReservesOp
            {
                SponsoredID = new AccountID(SponsoredId.XdrPublicKey)
            }
        };
    }

    /// <summary>
    ///     Builder for <c>BeginSponsoringFutureReserves</c>.
    /// </summary>
    public class Builder
    {
        private readonly KeyPair _sponsoredId;
        private KeyPair? _sourceAccount;

        /// <summary>
        ///     Constructs a new <c>BeginSponsoringFutureReserves</c> builder.
        /// </summary>
        /// <param name="xdrPublicKey">An <c>Xdr.PublicKey</c> object.</param>
        public Builder(PublicKey xdrPublicKey)
        {
            _sponsoredId = KeyPair.FromXdrPublicKey(xdrPublicKey);
        }

        /// <summary>
        ///     Constructs a new <c>BeginSponsoringFutureReserves</c> builder.
        /// </summary>
        /// <param name="sponsoredId">The Ed25519 public key of an account to be sponsored.</param>
        public Builder(string sponsoredId) : this(KeyPair.FromAccountId(sponsoredId))
        {
        }

        /// <summary>
        ///     Constructs a new <c>BeginSponsoringFutureReserves</c> builder.
        /// </summary>
        /// <param name="sponsoredAccount">The key pair of an account to be sponsored.</param>
        public Builder(KeyPair sponsoredAccount)
        {
            _sponsoredId = sponsoredAccount ?? throw new ArgumentNullException(nameof(sponsoredAccount));
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
        public BeginSponsoringFutureReservesOperation Build()
        {
            var operation = new BeginSponsoringFutureReservesOperation(_sponsoredId);
            if (_sourceAccount != null)
                operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}