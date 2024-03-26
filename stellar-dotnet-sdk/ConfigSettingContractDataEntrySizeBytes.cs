using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractDataEntrySizeBytes : LedgerEntryConfigSetting
{
    private ConfigSettingContractDataEntrySizeBytes(uint value)
    {
        InnerValue = value;
    }

    public uint InnerValue { get; }

    public static ConfigSettingContractDataEntrySizeBytes FromXdr(Uint32 xdrConfig)
    {
        return new ConfigSettingContractDataEntrySizeBytes(xdrConfig.InnerValue);
    }
}