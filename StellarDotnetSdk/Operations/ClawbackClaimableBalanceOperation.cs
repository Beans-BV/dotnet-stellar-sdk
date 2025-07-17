using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Claws back an unclaimed ClaimableBalanceEntry, burning the pending amount of the asset.
///     See:
///     <a
///         href="https://developers.stellar.org/docs/learn/fundamentals/transactions/list-of-operations#clawback-claimable-balance">
///         Clawback claimable balance
///     </a>
/// </summary>
public class ClawbackClaimableBalanceOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>ClawbackClaimableBalanceOperation</c>.
    /// </summary>
    /// <param name="balanceId">The hex-encoded ID (0000...) of the ClaimableBalanceEntry to be clawed back.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public ClawbackClaimableBalanceOperation(string balanceId, IAccountId? sourceAccount = null)
        : base(sourceAccount)
    {
        if (!StrKey.IsValidClaimableBalanceId(ClaimableBalanceUtils.ToBase32String(balanceId)))
        {
            throw new ArgumentException($"Invalid claimable balance ID {balanceId}");
        }
        BalanceId = balanceId;
    }

    /// <summary>
    ///     Hex-encoded ID (0000...) of the ClaimableBalanceEntry to be clawed back.
    /// </summary>
    public string BalanceId { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CLAWBACK_CLAIMABLE_BALANCE),
            ClawbackClaimableBalanceOp = new ClawbackClaimableBalanceOp
            {
                BalanceID = ClaimableBalanceUtils.FromHexString(BalanceId),
            },
        };
    }
}