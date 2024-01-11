using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractDataEntrySizeBytes : LedgerEntryConfigSetting
{
    public ConfigSettingContractDataEntrySizeBytes(uint value)
    {
        InnerValue = value;
    }

    public uint InnerValue { get; }

    public ConfigSettingEntry ToXdrConfigSettingEntry()
    {
        return new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(
                    ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_DATA_ENTRY_SIZE_BYTES),
            ContractDataEntrySizeBytes = new Uint32(InnerValue)
        };
    }
}