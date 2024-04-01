using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

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