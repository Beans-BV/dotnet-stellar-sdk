using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Terminates the current is-sponsoring-future-reserves relationship in which the source account is sponsored.
///     See:
///     <see href="https://developers.stellar.org/docs/encyclopedia/sponsored-reserves#begin-and-end-sponsorships">
///         Begin and end sponsorships
///     </see>
/// </summary>
public class EndSponsoringFutureReservesOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>EndSponsoringFutureReservesOperation</c>.
    /// </summary>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public EndSponsoringFutureReservesOperation(IAccountId? sourceAccount = null) : base(sourceAccount)
    {
    }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.END_SPONSORING_FUTURE_RESERVES),
        };
    }
}