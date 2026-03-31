using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

/// <summary>
///     Represents a ledger key for a claimable balance entry on the Stellar network.
///     Used to look up claimable balance data from the ledger by its unique balance ID.
/// </summary>
public class LedgerKeyClaimableBalance : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyClaimableBalance</c> from a claimable balance ID.
    /// </summary>
    /// <param name="balanceId">A hex-encoded claimable balance ID (0000...).</param>
    public LedgerKeyClaimableBalance(string balanceId)
    {
        if (!StrKey.IsValidClaimableBalanceId(ClaimableBalanceIdUtils.ToBase32String(balanceId)))
        {
            throw new ArgumentException($"Invalid claimable balance ID {balanceId}.");
        }
        BalanceId = balanceId;
    }

    /// <summary>
    ///     Hex-encoded claimable balance ID (0000...).
    /// </summary>
    public string BalanceId { get; }

    /// <summary>
    ///     Serializes this ledger key to its XDR representation.
    /// </summary>
    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE },
            ClaimableBalance = new Xdr.LedgerKey.LedgerKeyClaimableBalance
            {
                BalanceID = ClaimableBalanceIdUtils.FromHexString(BalanceId),
            },
        };
    }

    /// <summary>
    ///     Deserializes a <see cref="LedgerKeyClaimableBalance" /> from its XDR representation.
    /// </summary>
    /// <param name="xdr">The XDR ledger key claimable balance object.</param>
    public static LedgerKeyClaimableBalance FromXdr(Xdr.LedgerKey.LedgerKeyClaimableBalance xdr)
    {
        return new LedgerKeyClaimableBalance(
            ClaimableBalanceIdUtils.ToHexString(xdr.BalanceID)
        );
    }
}