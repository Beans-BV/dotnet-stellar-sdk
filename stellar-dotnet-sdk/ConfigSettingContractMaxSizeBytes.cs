using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractMaxSizeBytes : LedgerEntryConfigSetting
{
    private ConfigSettingContractMaxSizeBytes(uint value)
    {
        InnerValue = value;
    }

    public uint InnerValue { get; }

    public static ConfigSettingContractMaxSizeBytes FromXdr(Uint32 xdrConfig)
    {
        return new ConfigSettingContractMaxSizeBytes(xdrConfig.InnerValue);
    }
}