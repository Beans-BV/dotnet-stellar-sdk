using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration setting for the maximum size in bytes of a contract data key.
/// </summary>
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