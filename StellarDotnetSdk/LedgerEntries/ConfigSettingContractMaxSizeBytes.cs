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

    /// <summary>
    ///     The maximum size in bytes of a smart contract.
    /// </summary>
    public uint InnerValue { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingContractMaxSizeBytes" /> from an XDR <see cref="Uint32" /> value.
    /// </summary>
    /// <param name="xdrConfig">The XDR value.</param>
    /// <returns>A <see cref="ConfigSettingContractMaxSizeBytes" /> instance.</returns>
    public static ConfigSettingContractMaxSizeBytes FromXdr(Uint32 xdrConfig)
    {
        return new ConfigSettingContractMaxSizeBytes(xdrConfig.InnerValue);
    }
}