using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public abstract class LedgerEntry
{
    public uint LastModifiedLedgerSeq { get; set; }

    public LedgerEntryExtensionV1? LedgerExtensionV1 { get; set; }

    public xdr.LedgerEntry ToXdr()
    {
        var xdrLedgerEntry = new xdr.LedgerEntry
        {
            Data = new xdr.LedgerEntry.LedgerEntryData
            {
                Discriminant = new LedgerEntryType()
            },
            LastModifiedLedgerSeq = new Uint32(LastModifiedLedgerSeq),
            Ext = new xdr.LedgerEntry.LedgerEntryExt
            {
                Discriminant = LedgerExtensionV1 != null ? 1 : 0,
                V1 = LedgerExtensionV1?.ToXdr() ?? new xdr.LedgerEntryExtensionV1()
            }
        };

        switch (this)
        {
            case LedgerEntryAccount accountEntry:
                xdrLedgerEntry.Data.Discriminant.InnerValue = LedgerEntryType.LedgerEntryTypeEnum.ACCOUNT;
                xdrLedgerEntry.Data.Account = accountEntry.ToXdr();
                break;
            case LedgerEntryOffer offerEntry:
                xdrLedgerEntry.Data.Discriminant.InnerValue = LedgerEntryType.LedgerEntryTypeEnum.OFFER;
                xdrLedgerEntry.Data.Offer = offerEntry.ToXdr();
                break;
            case LedgerEntryTrustline trustlineEntry:
                xdrLedgerEntry.Data.Discriminant.InnerValue = LedgerEntryType.LedgerEntryTypeEnum.TRUSTLINE;
                xdrLedgerEntry.Data.TrustLine = trustlineEntry.ToXdr();
                break;
            case LedgerEntryData dataEntry:
                xdrLedgerEntry.Data.Discriminant.InnerValue = LedgerEntryType.LedgerEntryTypeEnum.DATA;
                xdrLedgerEntry.Data.Data = dataEntry.ToXdr();
                break;
            case LedgerEntryClaimableBalance claimableBalanceEntry:
                xdrLedgerEntry.Data.Discriminant.InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE;
                xdrLedgerEntry.Data.ClaimableBalance = claimableBalanceEntry.ToXdr();
                break;
            case LedgerEntryLiquidityPool liquidityPoolEntry:
                xdrLedgerEntry.Data.Discriminant.InnerValue = LedgerEntryType.LedgerEntryTypeEnum.LIQUIDITY_POOL;
                xdrLedgerEntry.Data.LiquidityPool = liquidityPoolEntry.ToXdr();
                break;
            case LedgerEntryContractData contractDataEntry:
                xdrLedgerEntry.Data.Discriminant.InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_DATA;
                xdrLedgerEntry.Data.ContractData = contractDataEntry.ToXdr();
                break;
            case LedgerEntryContractCode contractCodeEntry:
                xdrLedgerEntry.Data.Discriminant.InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_CODE;
                xdrLedgerEntry.Data.ContractCode = contractCodeEntry.ToXdr();
                break;
            case LedgerEntryConfigSetting configSettingEntry:
                xdrLedgerEntry.Data.Discriminant.InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING;
                xdrLedgerEntry.Data.ConfigSetting = configSettingEntry.ToXdr();
                break;
            case LedgerEntryTTL ttlEntry:
                xdrLedgerEntry.Data.Discriminant.InnerValue = LedgerEntryType.LedgerEntryTypeEnum.TTL;
                xdrLedgerEntry.Data.Ttl = ttlEntry.ToXdr();
                break;
            default:
                throw new InvalidOperationException("Unknown LedgerEntry type");
        }

        return xdrLedgerEntry;
    }

    public static LedgerEntry FromXdr(xdr.LedgerEntry xdrLedgerEntry)
    {
        return xdrLedgerEntry.Data.Discriminant.InnerValue switch
        {
            LedgerEntryType.LedgerEntryTypeEnum.ACCOUNT => LedgerEntryAccount.FromXdrLedgerEntry(xdrLedgerEntry),
            LedgerEntryType.LedgerEntryTypeEnum.TRUSTLINE => LedgerEntryTrustline.FromXdrLedgerEntry(xdrLedgerEntry),
            LedgerEntryType.LedgerEntryTypeEnum.OFFER => LedgerEntryOffer.FromXdrLedgerEntry(xdrLedgerEntry),
            LedgerEntryType.LedgerEntryTypeEnum.DATA => LedgerEntryData.FromXdrLedgerEntry(xdrLedgerEntry),
            LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE => LedgerEntryClaimableBalance.FromXdrLedgerEntry(xdrLedgerEntry),
            LedgerEntryType.LedgerEntryTypeEnum.LIQUIDITY_POOL => LedgerEntryLiquidityPool.FromXdrLedgerEntry(xdrLedgerEntry),
            LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_DATA => LedgerEntryContractData.FromXdrLedgerEntry(xdrLedgerEntry),
            LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_CODE => LedgerEntryContractCode.FromXdrLedgerEntry(xdrLedgerEntry),
            LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING => LedgerEntryConfigSetting.FromXdrLedgerEntry(xdrLedgerEntry),
            LedgerEntryType.LedgerEntryTypeEnum.TTL => LedgerEntryTTL.FromXdrLedgerEntry(xdrLedgerEntry),
            _ => throw new InvalidOperationException("Unknown LedgerEntry type")
        };
    }

    /// <summary>
    ///     Creates a new LedgerEntry object from the given LedgerEntry XDR base64 string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>LedgerEntry object</returns>
    public static LedgerEntry FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = xdr.LedgerEntry.Decode(reader);
        return FromXdr(thisXdr);
    }

    /// <summary>
    ///     Returns a base64-encoded string that represents the XDR (External Data Representation) format of a
    ///     <see cref="LedgerEntry" /> object.
    /// </summary>
    /// <returns>
    ///     A base64-encoded string that contains the XDR representation of the
    ///     <see cref="stellar_dotnet_sdk.xdr.LedgerEntry" /> object.
    /// </returns>
    public string ToXdrBase64()
    {
        var ledgerEntry = ToXdr();
        var writer = new XdrDataOutputStream();
        xdr.LedgerEntry.Encode(writer, ledgerEntry);
        return Convert.ToBase64String(writer.ToArray());
    }

    protected static void ExtraFieldsFromXdr(xdr.LedgerEntry xdrLedgerEntry, LedgerEntry ledgerEntry)
    {
        ledgerEntry.LastModifiedLedgerSeq = xdrLedgerEntry.LastModifiedLedgerSeq.InnerValue;
        if (xdrLedgerEntry.Ext.Discriminant == 1)
            ledgerEntry.LedgerExtensionV1 = LedgerEntryExtensionV1.FromXdr(xdrLedgerEntry.Ext.V1);
    }
}