using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Claims a ClaimableBalanceEntry that corresponds to the BalanceID and adds the amount of an asset on the entry to
///     the source account.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#claim-claimable-balance">
///         Claim claimable balance
///     </a>
/// </summary>
public class ClaimClaimableBalanceOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>ClaimClaimableBalanceOperation</c>.
    /// </summary>
    /// <param name="balanceId">The hex-encoded ID of the ClaimableBalanceEntry to be claimed.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public ClaimClaimableBalanceOperation(string balanceId, IAccountId? sourceAccount = null) : this(
        Util.HexToBytes(balanceId), sourceAccount)
    {
    }

    private ClaimClaimableBalanceOperation(byte[] balanceId, IAccountId? sourceAccount = null) : base(sourceAccount)
    {
        // Backwards compat - was previously expecting no type to be set, set CLAIMABLE_BALANCE_ID_TYPE_V0 to match previous behaviour.
        if (balanceId.Length == 32)
        {
            var expanded = new byte[36];
            Array.Copy(balanceId, 0, expanded, 4, 32);
            balanceId = expanded;
        }

        if (balanceId.Length != 36)
        {
            throw new ArgumentException("Must be 36 bytes long", nameof(balanceId));
        }

        BalanceIdInBytes = balanceId;
    }

    /// <summary>
    ///     Hex-encoded ID of the ClaimableBalanceEntry to be claimed.
    /// </summary>
    public string BalanceId => Util.BytesToHex(BalanceIdInBytes);

    /// <summary>
    ///     ID of the ClaimableBalanceEntry to be claimed as a byte array.
    /// </summary>
    public byte[] BalanceIdInBytes { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE),
            ClaimClaimableBalanceOp = new ClaimClaimableBalanceOp
            {
                BalanceID = ClaimableBalanceID.Decode(new XdrDataInputStream(BalanceIdInBytes)),
            },
        };
    }
}