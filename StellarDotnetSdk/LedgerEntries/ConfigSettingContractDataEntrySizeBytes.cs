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

    /// <summary>
    ///     The maximum size in bytes of a contract data entry.
    /// </summary>
    public uint InnerValue { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingContractDataEntrySizeBytes" /> from an XDR <see cref="Uint32" /> value.
    /// </summary>
    /// <param name="xdrConfig">The XDR value.</param>
    /// <returns>A <see cref="ConfigSettingContractDataEntrySizeBytes" /> instance.</returns>
    public static ConfigSettingContractDataEntrySizeBytes FromXdr(Uint32 xdrConfig)
    {
        return new ConfigSettingContractDataEntrySizeBytes(xdrConfig.InnerValue);
    }
}