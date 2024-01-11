using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractMaxSizeBytes : LedgerEntryConfigSetting
{
    public ConfigSettingContractMaxSizeBytes(uint value)
    {
        InnerValue = value;
    }

    public uint InnerValue { get; }

    public ConfigSettingEntry ToXdrConfigSettingEntry()
    {
        return new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_MAX_SIZE_BYTES),
            ContractMaxSizeBytes = new Uint32(InnerValue)
        };
    }
}