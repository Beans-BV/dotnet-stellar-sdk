using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Claims a ClaimableBalanceEntry that corresponds to the BalanceID and adds the amount of an asset on the entry to
///     the source account.
///     See:
///     <a
///         href="https://developers.stellar.org/docs/learn/fundamentals/transactions/list-of-operations#claim-claimable-balance">
///         Claim claimable balance
///     </a>
/// </summary>
public class ClaimClaimableBalanceOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>ClaimClaimableBalanceOperation</c>.
    /// </summary>
    /// <param name="balanceId">The hex-encoded ID (0000...) of the ClaimableBalanceEntry to be claimed.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public ClaimClaimableBalanceOperation(string balanceId, IAccountId? sourceAccount = null)
        : base(sourceAccount)
    {
        if (!StrKey.IsValidClaimableBalanceId(ClaimableBalanceUtils.ToBase32String(balanceId)))
        {
            throw new ArgumentException($"Invalid claimable balance ID {balanceId}");
        }
        BalanceId = balanceId;
    }

    /// <summary>
    ///     Hex-encoded ID (0000...) of the ClaimableBalanceEntry to be claimed.
    /// </summary>
    public string BalanceId { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE),
            ClaimClaimableBalanceOp = new ClaimClaimableBalanceOp
            {
                BalanceID = ClaimableBalanceUtils.FromHexString(BalanceId),
            },
        };
    }
}