using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractExecutionLanes : LedgerEntryConfigSetting
{
    public uint LedgerMaxTxCount { get; set; }

    public static ConfigSettingContractExecutionLanes FromXdr(xdr.ConfigSettingContractExecutionLanesV0 xdrConfig)
    {
        return new ConfigSettingContractExecutionLanes
        {
            LedgerMaxTxCount = xdrConfig.LedgerMaxTxCount.InnerValue
        };
    }

    public xdr.ConfigSettingContractExecutionLanesV0 ToXdr()
    {
        return new xdr.ConfigSettingContractExecutionLanesV0
        {
            LedgerMaxTxCount = new Uint32(LedgerMaxTxCount)
        };
    }

    public ConfigSettingEntry ToXdrConfigSettingEntry()
    {
        return new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_EXECUTION_LANES),
            ContractExecutionLanes = ToXdr()
        };
    }
}