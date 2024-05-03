using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

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