using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration settings for smart contract event size limits and fees.
/// </summary>
public class ConfigSettingContractEvents : LedgerEntryConfigSetting
{
    private ConfigSettingContractEvents(uint txMaxContractEventsSizeBytes, long feeContractEvents1Kb)
    {
        TxMaxContractEventsSizeBytes = txMaxContractEventsSizeBytes;
        FeeContractEvents1Kb = feeContractEvents1Kb;
    }

    /// <summary>
    ///     Maximum size of events that a contract call can emit, in bytes.
    /// </summary>
    public uint TxMaxContractEventsSizeBytes { get; }

    /// <summary>
    ///     Fee for generating 1KB of contract events.
    /// </summary>
    public long FeeContractEvents1Kb { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingContractEvents" /> from an XDR
    ///     <see cref="ConfigSettingContractEventsV0" /> object.
    /// </summary>
    /// <param name="xdrConfig">The XDR config setting object.</param>
    /// <returns>A <see cref="ConfigSettingContractEvents" /> instance.</returns>
    public static ConfigSettingContractEvents FromXdr(ConfigSettingContractEventsV0 xdrConfig)
    {
        return new ConfigSettingContractEvents(
            xdrConfig.TxMaxContractEventsSizeBytes.InnerValue,
            xdrConfig.FeeContractEvents1KB.InnerValue);
    }
}