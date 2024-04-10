using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

public class ConfigSettingContractLedgerCost : LedgerEntryConfigSetting
{
    private ConfigSettingContractLedgerCost(uint ledgerMaxReadLedgerEntries, uint ledgerMaxReadBytes,
        uint ledgerMaxWriteLedgerEntries, uint ledgerMaxWriteBytes, uint txMaxReadLedgerEntries, uint txMaxReadBytes,
        uint txMaxWriteLedgerEntries, uint txMaxWriteBytes, long feeReadLedgerEntry, long feeWriteLedgerEntry,
        long feeRead1Kb, long bucketListTargetSizeBytes, long writeFee1KbBucketListLow, long writeFee1KbBucketListHigh,
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
        FeeRead1Kb = feeRead1Kb;
        BucketListTargetSizeBytes = bucketListTargetSizeBytes;
        WriteFee1KbBucketListLow = writeFee1KbBucketListLow;
        WriteFee1KbBucketListHigh = writeFee1KbBucketListHigh;
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
    public long FeeRead1Kb { get; }
    public long BucketListTargetSizeBytes { get; }
    public long WriteFee1KbBucketListLow { get; }
    public long WriteFee1KbBucketListHigh { get; }
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