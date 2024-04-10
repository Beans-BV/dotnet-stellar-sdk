using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Claws back an unclaimed ClaimableBalanceEntry, burning the pending amount of the asset.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#clawback-claimable-balance">
///         Clawback claimable balance
///     </a>
/// </summary>
public class ClawbackClaimableBalanceOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>ClawbackClaimableBalanceOperation</c>.
    /// </summary>
    /// <param name="balanceIdInBytes">The hex-encoded ID of the ClaimableBalanceEntry to be clawed back.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public ClawbackClaimableBalanceOperation(string balanceId, IAccountId? sourceAccount = null) : base(sourceAccount)
    {
        var balanceIdInBytes = Util.HexToBytes(balanceId);
        // Backwards compatibility - was previously expecting no type to be set.
        if (balanceIdInBytes.Length == 32)
        {
            var expanded = new byte[36];
            Array.Copy(balanceIdInBytes, 0, expanded, 4, 32);
            balanceIdInBytes = expanded;
        }

        if (balanceIdInBytes.Length != 36)
            throw new ArgumentException("Must be 36 bytes long", nameof(balanceIdInBytes));

        BalanceIdInBytes = balanceIdInBytes;
    }

    /// <summary>
    ///     Hex-encoded ID of the ClaimableBalanceEntry to be clawed back.
    /// </summary>
    public string BalanceId => Util.BytesToHex(BalanceIdInBytes);

    /// <summary>
    ///     ID of the ClaimableBalanceEntry to be clawed back as a byte array.
    /// </summary>
    public byte[] BalanceIdInBytes { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CLAWBACK_CLAIMABLE_BALANCE),
            ClawbackClaimableBalanceOp = new ClawbackClaimableBalanceOp
            {
                BalanceID = ClaimableBalanceID.Decode(new XdrDataInputStream(BalanceIdInBytes))
            }
        };
    }
}