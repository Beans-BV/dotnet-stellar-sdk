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

    public uint LedgerMaxTxCount { get; }

    public static ConfigSettingContractExecutionLanes FromXdr(ConfigSettingContractExecutionLanesV0 xdrConfig)
    {
        return new ConfigSettingContractExecutionLanes(xdrConfig.LedgerMaxTxCount.InnerValue);
    }
}