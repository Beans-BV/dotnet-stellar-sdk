using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractExecutionLanesV0 : LedgerEntryConfigSetting
{
    public uint LedgerMaxTxCount { get; set; }

    public static ConfigSettingContractExecutionLanesV0 FromXdr(xdr.ConfigSettingContractExecutionLanesV0 xdrConfig)
    {
        return new ConfigSettingContractExecutionLanesV0
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