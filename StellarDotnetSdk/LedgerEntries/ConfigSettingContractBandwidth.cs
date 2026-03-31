using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration settings for smart contract transaction bandwidth limits and fees.
/// </summary>
public class ConfigSettingContractBandwidth : LedgerEntryConfigSetting
{
    private ConfigSettingContractBandwidth(uint ledgerMaxTxsSizeBytes, uint txMaxSizeBytes, long feeTxSize1Kb)
    {
        LedgerMaxTxsSizeBytes = ledgerMaxTxsSizeBytes;
        TxMaxSizeBytes = txMaxSizeBytes;
        FeeTxSize1Kb = feeTxSize1Kb;
    }

    /// <summary>
    ///     Maximum sum of all transaction sizes in the ledger, in bytes.
    /// </summary>
    public uint LedgerMaxTxsSizeBytes { get; }

    /// <summary>
    ///     Maximum size in bytes for a single transaction.
    /// </summary>
    public uint TxMaxSizeBytes { get; }

    /// <summary>
    ///     Fee for 1KB of transaction size.
    /// </summary>
    public long FeeTxSize1Kb { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingContractBandwidth" /> from an XDR
    ///     <see cref="ConfigSettingContractBandwidthV0" /> object.
    /// </summary>
    /// <param name="xdrConfig">The XDR config setting object.</param>
    /// <returns>A <see cref="ConfigSettingContractBandwidth" /> instance.</returns>
    public static ConfigSettingContractBandwidth FromXdr(ConfigSettingContractBandwidthV0 xdrConfig)
    {
        return new ConfigSettingContractBandwidth(
            xdrConfig.LedgerMaxTxsSizeBytes.InnerValue,
            xdrConfig.TxMaxSizeBytes.InnerValue,
            xdrConfig.FeeTxSize1KB.InnerValue);
    }
}