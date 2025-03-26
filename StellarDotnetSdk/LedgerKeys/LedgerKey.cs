using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;
using SCAddress = StellarDotnetSdk.Soroban.SCAddress;
using SCVal = StellarDotnetSdk.Soroban.SCVal;

namespace StellarDotnetSdk.LedgerKeys;

public abstract class LedgerKey
{
    public abstract Xdr.LedgerKey ToXdr();

    public static LedgerKey Account(KeyPair account)
    {
        return new LedgerKeyAccount(account);
    }

    [Obsolete("Deprecated. Use ClaimableBalance(string balanceIdHexString) instead.")]
    /// <summary>
    ///     Constructs a <c>LedgerKeyClaimableBalance</c> object from a 32-byte array representation of a Claimable balance ID v0.
    /// </summary>
    /// <param name="balanceIdV0ByteArray">Byte array representation of the claimable balance ID v0.</param>
    public static LedgerKey ClaimableBalance(byte[] balanceIdV0ByteArray)
    {
        return new LedgerKeyClaimableBalance(balanceIdV0ByteArray);
    }

    /// <summary>Constructs a new <c>LedgerKeyClaimableBalance</c> from the given hex-encoded claimable balance ID.</summary>
    /// <param name="balanceIdHexString">
    ///     Hex-encoded ID of the claimable balance entry.
    ///     For example: <c>00000000d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780</c>.
    /// </param>
    /// <remarks>
    ///     Balance ID and Balance ID v0 are not the same. The v0 values don't have 8 leading zeros.
    ///     For example, if balance ID is 00000000d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780,
    ///     then balance ID v0 is d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780.
    /// </remarks>
    public static LedgerKey ClaimableBalance(string balanceIdHexString)
    {
        return new LedgerKeyClaimableBalance(balanceIdHexString);
    }

    public static LedgerKey Data(KeyPair account, string dataName)
    {
        return new LedgerKeyData(account, dataName);
    }

    public static LedgerKey Offer(KeyPair seller, long offerId)
    {
        return new LedgerKeyOffer(seller, offerId);
    }

    public static LedgerKey Trustline(KeyPair account, TrustlineAsset asset)
    {
        return new LedgerKeyTrustline(account, asset);
    }

    public static LedgerKey LiquidityPool(LiquidityPoolId poolId)
    {
        return new LedgerKeyLiquidityPool(poolId);
    }

    public static LedgerKey ContractData(SCAddress contractId, SCVal key, ContractDataDurability durability)
    {
        return new LedgerKeyContractData(contractId, key, durability);
    }

    public static LedgerKey ContractCode(string code)
    {
        return new LedgerKeyContractCode(code);
    }

    public static LedgerKey ConfigSetting(ConfigSettingID settingId)
    {
        return new LedgerKeyConfigSetting(settingId);
    }

    public static LedgerKey Ttl(string key)
    {
        return new LedgerKeyTtl(key);
    }

    public static LedgerKey FromXdr(Xdr.LedgerKey xdr)
    {
        return xdr.Discriminant.InnerValue switch
        {
            LedgerEntryType.LedgerEntryTypeEnum.ACCOUNT => LedgerKeyAccount.FromXdr(xdr.Account),
            LedgerEntryType.LedgerEntryTypeEnum.DATA => LedgerKeyData.FromXdr(xdr.Data),
            LedgerEntryType.LedgerEntryTypeEnum.OFFER => LedgerKeyOffer.FromXdr(xdr.Offer),
            LedgerEntryType.LedgerEntryTypeEnum.TRUSTLINE => LedgerKeyTrustline.FromXdr(xdr.TrustLine),
            LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE => LedgerKeyClaimableBalance.FromXdr(
                xdr.ClaimableBalance),
            LedgerEntryType.LedgerEntryTypeEnum.LIQUIDITY_POOL => LedgerKeyLiquidityPool.FromXdr(xdr.LiquidityPool),
            LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_DATA => LedgerKeyContractData.FromXdr(xdr.ContractData),
            LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_CODE => LedgerKeyContractCode.FromXdr(xdr.ContractCode),
            LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING => LedgerKeyConfigSetting.FromXdr(xdr.ConfigSetting),
            LedgerEntryType.LedgerEntryTypeEnum.TTL => LedgerKeyTtl.FromXdr(xdr.Ttl),
            _ => throw new Exception("Unknown ledger key " + xdr.Discriminant.InnerValue),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="LedgerKey" /> object from the given LedgerEntry XDR base64 string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns><see cref="LedgerKey" /> object</returns>
    public static LedgerKey FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = Xdr.LedgerKey.Decode(reader);
        return FromXdr(thisXdr);
    }

    /// <summary>
    ///     Returns a base-64 encoded XDR string that represents the <see cref="Xdr.LedgerKey" /> object.
    /// </summary>
    public string ToXdrBase64()
    {
        var xdrValue = ToXdr();
        var writer = new XdrDataOutputStream();
        Xdr.LedgerKey.Encode(writer, xdrValue);
        return Convert.ToBase64String(writer.ToArray());
    }
}