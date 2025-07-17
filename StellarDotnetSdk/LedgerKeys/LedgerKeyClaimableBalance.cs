using System;
using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

public class LedgerKeyClaimableBalance : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyClaimableBalance</c> from a claimable balance ID (B...).
    /// </summary>
    /// <param name="balanceId">A base32-encoded claimable balance ID (B...).</param>
    public LedgerKeyClaimableBalance(string balanceId)
    {
        if (!StrKey.IsValidClaimableBalanceId(balanceId))
        {
            throw new ArgumentException($"Invalid claimable balance ID {balanceId}.");
        }
        BalanceId = balanceId;
    }

    public string BalanceId { get; }

    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE },
            ClaimableBalance = new Xdr.LedgerKey.LedgerKeyClaimableBalance
            {
                BalanceID = ClaimableBalanceUtils.ToXdr(BalanceId),
            },
        };
    }

    public static LedgerKeyClaimableBalance FromXdr(Xdr.LedgerKey.LedgerKeyClaimableBalance xdr)
    {
        return new LedgerKeyClaimableBalance(
            ClaimableBalanceUtils.FromXdr(xdr.BalanceID)
        );
    }
}