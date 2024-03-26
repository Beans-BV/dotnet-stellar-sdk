using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

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