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

    /// <summary>
    ///     The maximum size in bytes of a contract data key.
    /// </summary>
    public uint InnerValue { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingContractDataKeySizeBytes" /> from an XDR <see cref="Uint32" /> value.
    /// </summary>
    /// <param name="xdrConfig">The XDR value.</param>
    /// <returns>A <see cref="ConfigSettingContractDataKeySizeBytes" /> instance.</returns>
    public static ConfigSettingContractDataKeySizeBytes FromXdr(Uint32 xdrConfig)
    {
        return new ConfigSettingContractDataKeySizeBytes(xdrConfig.InnerValue);
    }
}