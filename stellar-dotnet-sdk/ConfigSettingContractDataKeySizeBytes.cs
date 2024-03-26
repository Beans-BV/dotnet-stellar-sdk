using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractDataKeySizeBytes : LedgerEntryConfigSetting
{
    private ConfigSettingContractDataKeySizeBytes(uint value)
    {
        InnerValue = value;
    }

    public uint InnerValue { get; }

    public static ConfigSettingContractDataKeySizeBytes FromXdr(Uint32 xdrConfig)
    {
        return new ConfigSettingContractDataKeySizeBytes(xdrConfig.InnerValue);
    }
}