using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractDataKeySizeBytes : LedgerEntryConfigSetting
{
    public ConfigSettingContractDataKeySizeBytes(uint value)
    {
        InnerValue = value;
    }

    public uint InnerValue { get; }

    public ConfigSettingEntry ToXdrConfigSettingEntry()
    {
        return new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_DATA_KEY_SIZE_BYTES),
            ContractDataKeySizeBytes = new Uint32(InnerValue)
        };
    }
}