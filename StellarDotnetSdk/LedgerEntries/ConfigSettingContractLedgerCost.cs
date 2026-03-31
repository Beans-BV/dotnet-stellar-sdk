using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration settings for smart contract ledger access costs, including read/write limits,
///     fees, and rent parameters.
/// </summary>
public class ConfigSettingContractLedgerCost : LedgerEntryConfigSetting
{
    private ConfigSettingContractLedgerCost(
        uint ledgerMaxDiskReadEntries,
        uint ledgerMaxDiskReadBytes,
        uint ledgerMaxWriteLedgerEntries,
        uint ledgerMaxWriteBytes,
        uint txMaxDiskReadEntries,
        uint txMaxDiskReadBytes,
        uint txMaxWriteLedgerEntries,
        uint txMaxWriteBytes,
        long feeDiskReadLedgerEntry,
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
        TxMaxDiskReadEntries = txMaxDiskReadEntries;
        TxMaxDiskReadBytes = txMaxDiskReadBytes;
        TxMaxWriteLedgerEntries = txMaxWriteLedgerEntries;
        TxMaxWriteBytes = txMaxWriteBytes;
        FeeDiskReadLedgerEntry = feeDiskReadLedgerEntry;
        FeeWriteLedgerEntry = feeWriteLedgerEntry;
        FeeDiskRead1Kb = feeDiskRead1Kb;
        SorobanStateTargetSizeBytes = sorobanStateTargetSizeBytes;
        RentFee1KbSorobanStateSizeLow = rentFee1KbSorobanStateSizeLow;
        RentFee1KbSorobanStateSizeHigh = rentFee1KbSorobanStateSizeHigh;
        SorobanStateRentFeeGrowthFactor = sorobanStateRentFeeGrowthFactor;
    }

    /// <summary>
    ///     Maximum number of disk entry read operations per ledger.
    /// </summary>
    public uint LedgerMaxDiskReadEntries { get; }

    /// <summary>
    ///     Maximum number of bytes of disk reads that can be performed per ledger.
    /// </summary>
    public uint LedgerMaxDiskReadBytes { get; }

    /// <summary>
    ///     Maximum number of ledger entry write operations per ledger.
    /// </summary>
    public uint LedgerMaxWriteLedgerEntries { get; }

    /// <summary>
    ///     Maximum number of bytes that can be written per ledger.
    /// </summary>
    public uint LedgerMaxWriteBytes { get; }

    /// <summary>
    ///     Maximum number of disk entry read operations per transaction.
    /// </summary>
    public uint TxMaxDiskReadEntries { get; }

    /// <summary>
    ///     Maximum number of bytes of disk reads that can be performed per transaction.
    /// </summary>
    public uint TxMaxDiskReadBytes { get; }

    /// <summary>
    ///     Maximum number of ledger entry write operations per transaction.
    /// </summary>
    public uint TxMaxWriteLedgerEntries { get; }

    /// <summary>
    ///     Maximum number of bytes that can be written per transaction.
    /// </summary>
    public uint TxMaxWriteBytes { get; }

    /// <summary>
    ///     Fee per disk ledger entry read.
    /// </summary>
    public long FeeDiskReadLedgerEntry { get; }

    /// <summary>
    ///     Fee per ledger entry write.
    /// </summary>
    public long FeeWriteLedgerEntry { get; }

    /// <summary>
    ///     Fee for reading 1KB of disk.
    /// </summary>
    public long FeeDiskRead1Kb { get; }

    /// <summary>
    ///     Rent fee grows linearly until Soroban state reaches this size in bytes.
    /// </summary>
    public long SorobanStateTargetSizeBytes { get; }

    /// <summary>
    ///     Fee per 1KB rent when the Soroban state is empty.
    /// </summary>
    public long RentFee1KbSorobanStateSizeLow { get; }

    /// <summary>
    ///     Fee per 1KB rent when the Soroban state has reached <see cref="SorobanStateTargetSizeBytes" />.
    /// </summary>
    public long RentFee1KbSorobanStateSizeHigh { get; }

    /// <summary>
    ///     Rent fee multiplier for any additional data past the first <see cref="SorobanStateTargetSizeBytes" />.
    /// </summary>
    public uint SorobanStateRentFeeGrowthFactor { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingContractLedgerCost" /> from an XDR
    ///     <see cref="ConfigSettingContractLedgerCostV0" /> object.
    /// </summary>
    /// <param name="xdrConfig">The XDR config setting object.</param>
    /// <returns>A <see cref="ConfigSettingContractLedgerCost" /> instance.</returns>
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