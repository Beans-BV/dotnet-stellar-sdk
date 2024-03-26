using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <see cref="BeginSponsoringFutureReservesOp" />.
///     Use <see cref="Builder" /> to create a new BeginSponsoringFutureReservesOperation.
///     See also:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations">
///         Begin Sponsoring Futures Reserves
///     </a>
/// </summary>
public class BeginSponsoringFutureReservesOperation : Operation
{
    private BeginSponsoringFutureReservesOperation(KeyPair sponsoredId)
    {
        SponsoredId = sponsoredId ?? throw new ArgumentNullException(nameof(sponsoredId));
    }

    public KeyPair SponsoredId { get; }

    public override xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new xdr.Operation.OperationBody
        {
            Discriminant =
                OperationType.Create(OperationType.OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES),
            BeginSponsoringFutureReservesOp = new BeginSponsoringFutureReservesOp
            {
                SponsoredID = new AccountID(SponsoredId.XdrPublicKey)
            }
        };
        return body;
    }

    /// <summary>
    ///     Builds BeginSponsoringFutureReserves operation.
    /// </summary>
    /// <see cref="BeginSponsoringFutureReservesOperation" />
    public class Builder
    {
        private readonly KeyPair _sponsoredId;
        private KeyPair? _sourceAccount;

        /// <summary>
        ///     Construct a new BeginSponsoringFutureReserves builder from a BeginSponsoringFutureReservesOp XDR.
        /// </summary>
        /// <param name="beginSponsoringFutureReservesOp"></param>
        public Builder(BeginSponsoringFutureReservesOp beginSponsoringFutureReservesOp)
        {
            _sponsoredId = KeyPair.FromXdrPublicKey(beginSponsoringFutureReservesOp.SponsoredID.InnerValue);
        }

        /// <summary>
        ///     Create a new BeginSponsoringFutureReserves builder with the given sponsoredId.
        /// </summary>
        /// <param name="sponsoredId"></param>
        public Builder(string sponsoredId) : this(KeyPair.FromAccountId(sponsoredId))
        {
        }

        /// <summary>
        ///     Create a new BeginSponsoringFutureReserves builder with the given key pair.
        /// </summary>
        /// <param name="sponsoredId"></param>
        public Builder(KeyPair sponsoredId)
        {
            _sponsoredId = sponsoredId ?? throw new ArgumentNullException(nameof(sponsoredId));
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
        public BeginSponsoringFutureReservesOperation Build()
        {
            var operation = new BeginSponsoringFutureReservesOperation(_sponsoredId);
            if (_sourceAccount != null)
                operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}