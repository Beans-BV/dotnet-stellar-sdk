using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration settings for smart contract execution lanes, including the maximum transaction
///     count per ledger.
/// </summary>
public class ConfigSettingContractExecutionLanes : LedgerEntryConfigSetting
{
    private ConfigSettingContractExecutionLanes(uint ledgerMaxTxCount)
    {
        LedgerMaxTxCount = ledgerMaxTxCount;
    }

    /// <summary>
    ///     Maximum number of Soroban transactions per ledger.
    /// </summary>
    public uint LedgerMaxTxCount { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingContractExecutionLanes" /> from an XDR
    ///     <see cref="ConfigSettingContractExecutionLanesV0" /> object.
    /// </summary>
    /// <param name="xdrConfig">The XDR config setting object.</param>
    /// <returns>A <see cref="ConfigSettingContractExecutionLanes" /> instance.</returns>
    public static ConfigSettingContractExecutionLanes FromXdr(ConfigSettingContractExecutionLanesV0 xdrConfig)
    {
        return new ConfigSettingContractExecutionLanes(xdrConfig.LedgerMaxTxCount.InnerValue);
    }
}