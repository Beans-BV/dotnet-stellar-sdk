using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractLedgerCostV0 : LedgerEntryConfigSetting
{
    public uint LedgerMaxReadLedgerEntries { get; set; }
    public uint LedgerMaxReadBytes { get; set; }
    public uint LedgerMaxWriteLedgerEntries { get; set; }
    public uint LedgerMaxWriteBytes { get; set; }
    public uint TxMaxReadLedgerEntries { get; set; }
    public uint TxMaxReadBytes { get; set; }
    public uint TxMaxWriteLedgerEntries { get; set; }
    public uint TxMaxWriteBytes { get; set; }
    public long FeeReadLedgerEntry { get; set; }
    public long FeeWriteLedgerEntry { get; set; }
    public long FeeRead1KB { get; set; }
    public long BucketListTargetSizeBytes { get; set; }
    public long WriteFee1KBBucketListLow { get; set; }
    public long WriteFee1KBBucketListHigh { get; set; }
    public uint BucketListWriteFeeGrowthFactor { get; set; }

    public static ConfigSettingContractLedgerCostV0 FromXdr(xdr.ConfigSettingContractLedgerCostV0 xdrConfig)
    {
        return new ConfigSettingContractLedgerCostV0
        {
            LedgerMaxReadLedgerEntries = xdrConfig.LedgerMaxReadLedgerEntries.InnerValue,
            LedgerMaxReadBytes = xdrConfig.LedgerMaxReadBytes.InnerValue,
            LedgerMaxWriteLedgerEntries = xdrConfig.LedgerMaxWriteLedgerEntries.InnerValue,
            LedgerMaxWriteBytes = xdrConfig.LedgerMaxWriteBytes.InnerValue,
            TxMaxReadLedgerEntries = xdrConfig.TxMaxReadLedgerEntries.InnerValue,
            TxMaxReadBytes = xdrConfig.TxMaxReadBytes.InnerValue,
            TxMaxWriteLedgerEntries = xdrConfig.TxMaxWriteLedgerEntries.InnerValue,
            TxMaxWriteBytes = xdrConfig.TxMaxWriteBytes.InnerValue,
            FeeReadLedgerEntry = xdrConfig.FeeReadLedgerEntry.InnerValue,
            FeeWriteLedgerEntry = xdrConfig.FeeWriteLedgerEntry.InnerValue,
            FeeRead1KB = xdrConfig.FeeRead1KB.InnerValue,
            BucketListTargetSizeBytes = xdrConfig.BucketListTargetSizeBytes.InnerValue,
            WriteFee1KBBucketListLow = xdrConfig.WriteFee1KBBucketListLow.InnerValue,
            WriteFee1KBBucketListHigh = xdrConfig.WriteFee1KBBucketListHigh.InnerValue,
            BucketListWriteFeeGrowthFactor = xdrConfig.BucketListWriteFeeGrowthFactor.InnerValue
        };
    }

    public xdr.ConfigSettingContractLedgerCostV0 ToXdr()
    {
        return new xdr.ConfigSettingContractLedgerCostV0
        {
            LedgerMaxReadLedgerEntries = new Uint32(LedgerMaxReadLedgerEntries),
            LedgerMaxReadBytes = new Uint32(LedgerMaxReadBytes),
            LedgerMaxWriteLedgerEntries = new Uint32(LedgerMaxWriteLedgerEntries),
            LedgerMaxWriteBytes = new Uint32(LedgerMaxWriteBytes),
            TxMaxReadLedgerEntries = new Uint32(TxMaxReadLedgerEntries),
            TxMaxReadBytes = new Uint32(TxMaxReadBytes),
            TxMaxWriteLedgerEntries = new Uint32(TxMaxWriteLedgerEntries),
            TxMaxWriteBytes = new Uint32(TxMaxWriteBytes),
            FeeReadLedgerEntry = new Int64(FeeReadLedgerEntry),
            FeeWriteLedgerEntry = new Int64(FeeWriteLedgerEntry),
            FeeRead1KB = new Int64(FeeRead1KB),
            BucketListTargetSizeBytes = new Int64(BucketListTargetSizeBytes),
            WriteFee1KBBucketListLow = new Int64(WriteFee1KBBucketListLow),
            WriteFee1KBBucketListHigh = new Int64(WriteFee1KBBucketListHigh),
            BucketListWriteFeeGrowthFactor = new Uint32(BucketListWriteFeeGrowthFactor)
        };
    }

    public ConfigSettingEntry ToXdrConfigSettingEntry()
    {
        return new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_LEDGER_COST_V0),
            ContractLedgerCost = ToXdr()
        };
    }
}