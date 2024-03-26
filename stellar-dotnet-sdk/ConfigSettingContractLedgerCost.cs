using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractLedgerCost : LedgerEntryConfigSetting
{
    private ConfigSettingContractLedgerCost(uint ledgerMaxReadLedgerEntries, uint ledgerMaxReadBytes,
        uint ledgerMaxWriteLedgerEntries, uint ledgerMaxWriteBytes, uint txMaxReadLedgerEntries, uint txMaxReadBytes,
        uint txMaxWriteLedgerEntries, uint txMaxWriteBytes, long feeReadLedgerEntry, long feeWriteLedgerEntry,
        long feeRead1KB, long bucketListTargetSizeBytes, long writeFee1KBBucketListLow, long writeFee1KBBucketListHigh,
        uint bucketListWriteFeeGrowthFactor)
    {
        LedgerMaxReadLedgerEntries = ledgerMaxReadLedgerEntries;
        LedgerMaxReadBytes = ledgerMaxReadBytes;
        LedgerMaxWriteLedgerEntries = ledgerMaxWriteLedgerEntries;
        LedgerMaxWriteBytes = ledgerMaxWriteBytes;
        TxMaxReadLedgerEntries = txMaxReadLedgerEntries;
        TxMaxReadBytes = txMaxReadBytes;
        TxMaxWriteLedgerEntries = txMaxWriteLedgerEntries;
        TxMaxWriteBytes = txMaxWriteBytes;
        FeeReadLedgerEntry = feeReadLedgerEntry;
        FeeWriteLedgerEntry = feeWriteLedgerEntry;
        FeeRead1KB = feeRead1KB;
        BucketListTargetSizeBytes = bucketListTargetSizeBytes;
        WriteFee1KBBucketListLow = writeFee1KBBucketListLow;
        WriteFee1KBBucketListHigh = writeFee1KBBucketListHigh;
        BucketListWriteFeeGrowthFactor = bucketListWriteFeeGrowthFactor;
    }

    public uint LedgerMaxReadLedgerEntries { get; }
    public uint LedgerMaxReadBytes { get; }
    public uint LedgerMaxWriteLedgerEntries { get; }
    public uint LedgerMaxWriteBytes { get; }
    public uint TxMaxReadLedgerEntries { get; }
    public uint TxMaxReadBytes { get; }
    public uint TxMaxWriteLedgerEntries { get; }
    public uint TxMaxWriteBytes { get; }
    public long FeeReadLedgerEntry { get; }
    public long FeeWriteLedgerEntry { get; }
    public long FeeRead1KB { get; }
    public long BucketListTargetSizeBytes { get; }
    public long WriteFee1KBBucketListLow { get; }
    public long WriteFee1KBBucketListHigh { get; }
    public uint BucketListWriteFeeGrowthFactor { get; }

    public static ConfigSettingContractLedgerCost FromXdr(ConfigSettingContractLedgerCostV0 xdrConfig)
    {
        return new ConfigSettingContractLedgerCost(
            xdrConfig.LedgerMaxReadLedgerEntries.InnerValue,
            xdrConfig.LedgerMaxReadBytes.InnerValue,
            xdrConfig.LedgerMaxWriteLedgerEntries.InnerValue,
            xdrConfig.LedgerMaxWriteBytes.InnerValue,
            xdrConfig.TxMaxReadLedgerEntries.InnerValue,
            xdrConfig.TxMaxReadBytes.InnerValue,
            xdrConfig.TxMaxWriteLedgerEntries.InnerValue,
            xdrConfig.TxMaxWriteBytes.InnerValue,
            xdrConfig.FeeReadLedgerEntry.InnerValue,
            xdrConfig.FeeWriteLedgerEntry.InnerValue,
            xdrConfig.FeeRead1KB.InnerValue,
            xdrConfig.BucketListTargetSizeBytes.InnerValue,
            xdrConfig.WriteFee1KBBucketListLow.InnerValue,
            xdrConfig.WriteFee1KBBucketListHigh.InnerValue,
            xdrConfig.BucketListWriteFeeGrowthFactor.InnerValue);
    }
}