using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

public class LedgerKeyClaimableBalance : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyClaimableBalance</c> object from a 32-byte array.
    /// </summary>
    /// <param name="balanceIdByteArray">Byte array representation of the claimable balance entry.</param>
    public LedgerKeyClaimableBalance(byte[] balanceIdByteArray)
    {
        if (balanceIdByteArray.Length != 32)
        {
            throw new ArgumentException("Claimable balance ID byte array must have exactly 32 bytes.", nameof(balanceIdByteArray));
        }
        BalanceId = Convert.ToHexString(balanceIdByteArray);
    }

    /// <summary>
    ///     Constructs a <c>LedgerKeyClaimableBalance</c> from given hex-encoded claimable balance ID.
    /// </summary>
    /// <param name="balanceIdHexString">
    ///     Hex-encoded ID of the claimable balance entry.
    ///     For example: <c>d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780</c>.
    /// </param>
    public LedgerKeyClaimableBalance(string balanceIdHexString)
    {
        if (balanceIdHexString.Length > 64)
        {
            throw new ArgumentException("Claimable balance ID cannot exceed 64 characters.", nameof(balanceIdHexString));
        }
        BalanceId = balanceIdHexString;
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