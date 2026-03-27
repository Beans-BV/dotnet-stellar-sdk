using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration setting for the maximum size in bytes of a contract data entry.
/// </summary>
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