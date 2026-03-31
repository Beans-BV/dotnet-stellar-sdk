using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using SCVal = StellarDotnetSdk.Soroban.SCVal;

namespace StellarDotnetSdk.LedgerKeys;

/// <summary>
///     Base class for all ledger key types on the Stellar network.
///     A ledger key uniquely identifies a specific entry in the Stellar ledger by its type and associated fields.
/// </summary>
public abstract class LedgerKey
{
    /// <summary>
    ///     Serializes this ledger key to its XDR representation.
    /// </summary>
    public abstract Xdr.LedgerKey ToXdr();

    /// <summary>
    ///     Creates a ledger key that identifies an account entry.
    /// </summary>
    /// <param name="account">The key pair of the account.</param>
    public static LedgerKey Account(KeyPair account)
    {
        return new LedgerKeyAccount(account);
    }

    /// <summary>Constructs a new <c>LedgerKeyClaimableBalance</c> from a claimable balance ID.</summary>
    /// <param name="balanceId">A hex-encoded claimable balance ID (0000...).</param>
    public static LedgerKey ClaimableBalance(string balanceId)
    {
        return new LedgerKeyClaimableBalance(balanceId);
    }

    /// <summary>
    ///     Creates a ledger key that identifies a data entry attached to an account.
    /// </summary>
    /// <param name="account">The key pair of the account that owns the data entry.</param>
    /// <param name="dataName">The name of the data entry (max 64 characters).</param>
    public static LedgerKey Data(KeyPair account, string dataName)
    {
        return new LedgerKeyData(account, dataName);
    }

    /// <summary>
    ///     Creates a ledger key that identifies an offer entry on the order book.
    /// </summary>
    /// <param name="seller">The key pair of the account that created the offer.</param>
    /// <param name="offerId">The unique identifier of the offer.</param>
    public static LedgerKey Offer(KeyPair seller, long offerId)
    {
        return new LedgerKeyOffer(seller, offerId);
    }

    /// <summary>
    ///     Creates a ledger key that identifies a trustline entry.
    /// </summary>
    /// <param name="account">The key pair of the account that holds the trustline.</param>
    /// <param name="asset">The trustline asset.</param>
    public static LedgerKey Trustline(KeyPair account, TrustlineAsset asset)
    {
        return new LedgerKeyTrustline(account, asset);
    }

    /// <summary>
    ///     Creates a ledger key that identifies a liquidity pool entry.
    /// </summary>
    /// <param name="poolId">The unique identifier of the liquidity pool.</param>
    public static LedgerKey LiquidityPool(LiquidityPoolId poolId)
    {
        return new LedgerKeyLiquidityPool(poolId);
    }

    /// <summary>
    ///     Creates a ledger key that identifies a Soroban contract data entry.
    /// </summary>
    /// <param name="contractId">The address of the smart contract.</param>
    /// <param name="key">The key of the data entry within the contract's storage.</param>
    /// <param name="durability">The durability type (persistent or temporary) of the data entry.</param>
    public static LedgerKey ContractData(ScAddress contractId, SCVal key, ContractDataDurability durability)
    {
        return new LedgerKeyContractData(contractId, key, durability);
    }

    /// <summary>
    ///     Creates a ledger key that identifies a Soroban contract code (WASM bytecode) entry.
    /// </summary>
    /// <param name="code">The hex-encoded SHA-256 hash of the contract WASM bytecode.</param>
    public static LedgerKey ContractCode(string code)
    {
        return new LedgerKeyContractCode(code);
    }

    /// <summary>
    ///     Creates a ledger key that identifies a network configuration setting entry.
    /// </summary>
    /// <param name="settingId">The configuration setting identifier.</param>
    public static LedgerKey ConfigSetting(ConfigSettingID settingId)
    {
        return new LedgerKeyConfigSetting(settingId);
    }

    /// <summary>
    ///     Creates a ledger key that identifies a time-to-live (TTL) entry for Soroban contract data or code.
    /// </summary>
    /// <param name="key">A base64-encoded 32-byte hash of the associated contract data or code entry.</param>
    public static LedgerKey Ttl(string key)
    {
        return new LedgerKeyTtl(key);
    }

    /// <summary>
    ///     Deserializes a <see cref="LedgerKey" /> from its XDR representation.
    /// </summary>
    /// <param name="xdr">The XDR ledger key object.</param>
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