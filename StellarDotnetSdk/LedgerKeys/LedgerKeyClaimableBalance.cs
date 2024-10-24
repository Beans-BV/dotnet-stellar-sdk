using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

public class LedgerKeyClaimableBalance : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyClaimableBalance</c> object from a byte array.
    /// </summary>
    /// <param name="balanceId"></param>
    public LedgerKeyClaimableBalance(byte[] balanceId)
    {
        BalanceId = balanceId;
    }

    /// <summary>
    ///     Constructs a <c>LedgerKeyClaimableBalance</c> object from a hex-encoded string.
    /// </summary>
    /// <param name="balanceId">Hex-encoded string of a claimable balance ID.</param>
    public LedgerKeyClaimableBalance(string balanceId)
    {
        BalanceId = Convert.FromHexString(balanceId);
    }

    // TODO: Considering changing this to string
    public byte[] BalanceId { get; }

    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE },
            ClaimableBalance = new Xdr.LedgerKey.LedgerKeyClaimableBalance
            {
                BalanceID = new ClaimableBalanceID
                {
                    Discriminant = new ClaimableBalanceIDType
                    {
                        InnerValue = ClaimableBalanceIDType.ClaimableBalanceIDTypeEnum.CLAIMABLE_BALANCE_ID_TYPE_V0,
                    },
                    V0 = new Hash(BalanceId),
                },
            },
        };
    }

    public static LedgerKeyClaimableBalance FromXdr(Xdr.LedgerKey.LedgerKeyClaimableBalance xdr)
    {
        var balanceId = xdr.BalanceID.V0.InnerValue;
        return new LedgerKeyClaimableBalance(balanceId);
    }
}