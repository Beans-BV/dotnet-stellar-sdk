using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

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