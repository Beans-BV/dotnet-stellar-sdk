using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

public class ConfigSettingContractLedgerCost : LedgerEntryConfigSetting
{
    private ConfigSettingContractLedgerCost(
        uint ledgerMaxDiskReadEntries,
        uint ledgerMaxDiskReadBytes,
        uint ledgerMaxWriteLedgerEntries,
        uint ledgerMaxWriteBytes,
        uint txMaxReadLedgerEntries,
        uint txMaxDiskReadBytes,
        uint txMaxWriteLedgerEntries,
        uint txMaxWriteBytes,
        long feeReadLedgerEntry,
        long feeWriteLedgerEntry,
        long feeDiskRead1Kb,
        long sorobanStateTargetSizeBytes,
        long rentFee1KbSorobanStateSizeLow,
        long rentFee1KbSorobanStateSizeHigh,
        uint sorobanStateRentFeeGrowthFactor
    )
    {
        LedgerMaxDiskReadEntries = ledgerMaxDiskReadEntries;
        LedgerMaxDiskReadBytes = ledgerMaxDiskReadBytes;
        LedgerMaxWriteLedgerEntries = ledgerMaxWriteLedgerEntries;
        LedgerMaxWriteBytes = ledgerMaxWriteBytes;
        TxMaxReadLedgerEntries = txMaxReadLedgerEntries;
        TxMaxDiskReadBytes = txMaxDiskReadBytes;
        TxMaxWriteLedgerEntries = txMaxWriteLedgerEntries;
        TxMaxWriteBytes = txMaxWriteBytes;
        FeeReadLedgerEntry = feeReadLedgerEntry;
        FeeWriteLedgerEntry = feeWriteLedgerEntry;
        FeeDiskRead1Kb = feeDiskRead1Kb;
        SorobanStateTargetSizeBytes = sorobanStateTargetSizeBytes;
        RentFee1KbSorobanStateSizeLow = rentFee1KbSorobanStateSizeLow;
        RentFee1KbSorobanStateSizeHigh = rentFee1KbSorobanStateSizeHigh;
        SorobanStateRentFeeGrowthFactor = sorobanStateRentFeeGrowthFactor;
    }

    public uint LedgerMaxDiskReadEntries { get; }
    public uint LedgerMaxDiskReadBytes { get; }
    public uint LedgerMaxWriteLedgerEntries { get; }
    public uint LedgerMaxWriteBytes { get; }
    public uint TxMaxReadLedgerEntries { get; }
    public uint TxMaxDiskReadBytes { get; }
    public uint TxMaxWriteLedgerEntries { get; }
    public uint TxMaxWriteBytes { get; }
    public long FeeReadLedgerEntry { get; }
    public long FeeWriteLedgerEntry { get; }
    public long FeeDiskRead1Kb { get; }
    public long SorobanStateTargetSizeBytes { get; }
    public long RentFee1KbSorobanStateSizeLow { get; }
    public long RentFee1KbSorobanStateSizeHigh { get; }
    public uint SorobanStateRentFeeGrowthFactor { get; }

    public static ConfigSettingContractLedgerCost FromXdr(ConfigSettingContractLedgerCostV0 xdrConfig)
    {
        return new ConfigSettingContractLedgerCost(
            xdrConfig.LedgerMaxDiskReadEntries.InnerValue,
            xdrConfig.LedgerMaxDiskReadBytes.InnerValue,
            xdrConfig.LedgerMaxWriteLedgerEntries.InnerValue,
            xdrConfig.LedgerMaxWriteBytes.InnerValue,
            xdrConfig.TxMaxDiskReadEntries.InnerValue,
            xdrConfig.TxMaxDiskReadBytes.InnerValue,
            xdrConfig.TxMaxWriteLedgerEntries.InnerValue,
            xdrConfig.TxMaxWriteBytes.InnerValue,
            xdrConfig.FeeDiskReadLedgerEntry.InnerValue,
            xdrConfig.FeeWriteLedgerEntry.InnerValue,
            xdrConfig.FeeDiskRead1KB.InnerValue,
            xdrConfig.SorobanStateTargetSizeBytes.InnerValue,
            xdrConfig.RentFee1KBSorobanStateSizeLow.InnerValue,
            xdrConfig.RentFee1KBSorobanStateSizeHigh.InnerValue,
            xdrConfig.SorobanStateRentFeeGrowthFactor.InnerValue
        );
    }
}