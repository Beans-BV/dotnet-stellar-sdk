using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

public class LedgerKeyClaimableBalance : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyClaimableBalance</c> object from a 32-byte array.
    /// </summary>
    /// <param name="balanceId">Byte array representation of the claimable balance entry.</param>
    public LedgerKeyClaimableBalance(byte[] balanceId)
    {
        if (balanceId.Length != 32)
        {
            throw new ArgumentException("balanceId must have exactly 32 bytes.", nameof(balanceId));
        }
        BalanceId = Convert.ToHexString(balanceId);
    }

    /// <summary>
    ///     Constructs a <c>LedgerKeyClaimableBalance</c> from given hex-encoded claimable balance ID.
    /// </summary>
    /// <param name="balanceId">
    ///     Hex-encoded ID of the claimable balance entry.
    ///     For example: <c>d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780</c>.
    /// </param>
    public LedgerKeyClaimableBalance(string balanceId)
    {
        if (balanceId.Length > 64)
        {
            throw new ArgumentException("balanceId cannot exceed 64 characters.", nameof(balanceId));
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