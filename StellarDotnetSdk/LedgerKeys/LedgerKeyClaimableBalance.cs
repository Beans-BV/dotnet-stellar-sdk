using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

public class LedgerKeyClaimableBalance : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyClaimableBalance</c> object from a byte array.
    /// </summary>
    /// <param name="balanceId">Byte array representation of the claimable balance entry.</param>
    public LedgerKeyClaimableBalance(byte[] balanceId)
    {
        BalanceId = Convert.ToHexString(balanceId);
    }

    /// <summary>
    ///     Constructs a <c>LedgerKeyClaimableBalance</c> from given hex-encoded claimable balance ID.
    /// </summary>
    /// <param name="balanceId">
    ///     Hex-encoded ID of the claimable balance entry.
    ///     For example:
    ///     Either <c>00000000d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780</c>
    ///     or
    ///     <c>d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780</c> is accepted.
    /// </param>
    public LedgerKeyClaimableBalance(string balanceId)
    {
        // No idea why the balanceId is prefixed with 8 zeros in the Stellar Lab, but we need to remove them
        balanceId = balanceId.Length == 72 && balanceId.StartsWith("00000000") ? balanceId[8..] : balanceId;
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
                BalanceID = new ClaimableBalanceID
                {
                    Discriminant = new ClaimableBalanceIDType
                    {
                        InnerValue = ClaimableBalanceIDType.ClaimableBalanceIDTypeEnum.CLAIMABLE_BALANCE_ID_TYPE_V0,
                    },
                    V0 = new Hash(Convert.FromHexString(BalanceId)),
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