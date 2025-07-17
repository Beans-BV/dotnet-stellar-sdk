using System;
using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

public class LedgerKeyClaimableBalance : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyClaimableBalance</c> from a claimable balance ID.
    /// </summary>
    /// <param name="balanceId">A hex-encoded claimable balance ID (0000...).</param>
    public LedgerKeyClaimableBalance(string balanceId)
    {
        if (!StrKey.IsValidClaimableBalanceId(ClaimableBalanceUtils.ToBase32String(balanceId)))
        {
            throw new ArgumentException($"Invalid claimable balance ID {balanceId}.");
        }
        BalanceId = balanceId;
    }

    /// <summary>
    /// Hex-encoded claimable balance ID (0000...). 
    /// </summary>
    public string BalanceId { get; }

    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE },
            ClaimableBalance = new Xdr.LedgerKey.LedgerKeyClaimableBalance
            {
                BalanceID = ClaimableBalanceUtils.FromHexString(BalanceId),
            },
        };
    }

    public static LedgerKeyClaimableBalance FromXdr(Xdr.LedgerKey.LedgerKeyClaimableBalance xdr)
    {
        return new LedgerKeyClaimableBalance(
            ClaimableBalanceUtils.ToHexString(xdr.BalanceID)
        );
    }
}