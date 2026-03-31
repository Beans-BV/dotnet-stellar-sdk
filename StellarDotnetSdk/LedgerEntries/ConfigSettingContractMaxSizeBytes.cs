using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration setting for the maximum size in bytes of a smart contract.
/// </summary>
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