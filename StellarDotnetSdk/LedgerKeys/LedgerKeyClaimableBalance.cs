using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

public class LedgerKeyClaimableBalance : LedgerKey
{
    private readonly ClaimableBalanceID _xdrClaimableBalanceId;

    private LedgerKeyClaimableBalance(ClaimableBalanceID xdrClaimableBalanceId)
    {
        _xdrClaimableBalanceId = xdrClaimableBalanceId;
    }

    /// <summary>
    ///     Constructs a <c>LedgerKeyClaimableBalance</c> object from a 32-byte array representation of a Claimable balance ID
    ///     v0.
    /// </summary>
    /// <param name="balanceIdV0ByteArray">Byte array representation of the claimable balance ID v0.</param>
    public LedgerKeyClaimableBalance(byte[] balanceIdV0ByteArray)
    {
        if (balanceIdV0ByteArray.Length != 32)
        {
            throw new ArgumentException("Claimable balance ID V0 byte array must have exactly 32 bytes.",
                nameof(balanceIdV0ByteArray));
        }
        _xdrClaimableBalanceId = new ClaimableBalanceID
        {
            Discriminant = new ClaimableBalanceIDType
            {
                InnerValue = ClaimableBalanceIDType.ClaimableBalanceIDTypeEnum.CLAIMABLE_BALANCE_ID_TYPE_V0,
            },
            V0 = new Hash(balanceIdV0ByteArray),
        };
    }

    /// <summary>
    ///     Constructs a <c>LedgerKeyClaimableBalance</c> from given hex-encoded claimable balance ID.
    /// </summary>
    /// <param name="balanceIdHexString">
    ///     Hex-encoded ID of the claimable balance entry.
    ///     For example: <c>00000000d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780</c>.
    /// </param>
    public LedgerKeyClaimableBalance(string balanceIdHexString)
    {
        var bytes = Convert.FromHexString(balanceIdHexString);
        var inputStream = new XdrDataInputStream(bytes);
        _xdrClaimableBalanceId = ClaimableBalanceID.Decode(inputStream);
    }

    /// <summary>
    ///     Hex-encoded ID of the claimable balance entry.
    ///     For example: <c>00000000d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780</c>.
    /// </summary>
    public string BalanceId
    {
        get
        {
            var outputStream = new XdrDataOutputStream();
            ClaimableBalanceID.Encode(outputStream, _xdrClaimableBalanceId);
            return Convert.ToHexString(outputStream.ToArray());
        }
    }

    /// <summary>
    ///     Constructs a <c>LedgerKeyClaimableBalance</c> from given hex-encoded claimable balance ID v0.
    /// </summary>
    /// <param name="balanceIdV0">
    ///     Hex-encoded ID of the claimable balance v0 entry.
    ///     For example: <c>d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780</c>.
    /// </param>
    /// <remarks>
    ///     Balance ID and Balance ID v0 are not the same. The v0 values don't have 8 leading zeros.
    ///     For example, if balance ID is 00000000d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780,
    ///     then balance ID v0 is d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780.
    /// </remarks>
    public static LedgerKeyClaimableBalance FromBalanceIdV0(string balanceIdV0)
    {
        var xdrClaimableBalanceId = new ClaimableBalanceID
        {
            Discriminant = new ClaimableBalanceIDType
            {
                InnerValue = ClaimableBalanceIDType.ClaimableBalanceIDTypeEnum.CLAIMABLE_BALANCE_ID_TYPE_V0,
            },
            V0 = new Hash(Convert.FromHexString(balanceIdV0)),
        };
        return new LedgerKeyClaimableBalance(xdrClaimableBalanceId);
    }

    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant = new LedgerEntryType
            {
                InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE,
            },
            ClaimableBalance = new Xdr.LedgerKey.LedgerKeyClaimableBalance
            {
                BalanceID = _xdrClaimableBalanceId,
            },
        };
    }

    public static LedgerKeyClaimableBalance FromXdr(Xdr.LedgerKey.LedgerKeyClaimableBalance xdr)
    {
        var balanceId = xdr.BalanceID.V0.InnerValue;
        return new LedgerKeyClaimableBalance(balanceId);
    }
}