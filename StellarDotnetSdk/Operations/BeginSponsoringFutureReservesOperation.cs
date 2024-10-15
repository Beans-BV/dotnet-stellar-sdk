using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     <p>
///         Allows an account to pay the base reserves for another account; sponsoring account establishes the
///         is-sponsoring-future-reserves relationship.
///     </p>
///     <p>There must also be an <see cref="EndSponsoringFutureReservesOperation" /> in the same transaction.</p>
///     <p>
///         See:
///         <a
///             href="https://developers.stellar.org/docs/learn/fundamentals/transactions/list-of-operations#begin-sponsoring-future-reserves">
///             Begin sponsoring future reserves
///         </a>
///     </p>
/// </summary>
public class BeginSponsoringFutureReservesOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>BeginSponsoringFutureReservesOperation</c>.
    /// </summary>
    /// <param name="sponsoredId">The Ed25519 public key of an account to be sponsored.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public BeginSponsoringFutureReservesOperation(string sponsoredId, IAccountId? sourceAccount = null) : this(
        KeyPair.FromAccountId(sponsoredId), sourceAccount)
    {
    }

    /// <summary>
    ///     Constructs a new <c>BeginSponsoringFutureReservesOperation</c>.
    /// </summary>
    /// <param name="sponsoredAccount">Key pair of the account to be sponsored.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public BeginSponsoringFutureReservesOperation(KeyPair sponsoredAccount, IAccountId? sourceAccount = null) :
        base(sourceAccount)
    {
        SponsoredId = sponsoredAccount ?? throw new ArgumentNullException(nameof(sponsoredAccount));
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
                SponsoredID = new AccountID(SponsoredId.XdrPublicKey),
            },
        };
    }
}