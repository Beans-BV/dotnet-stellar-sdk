using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration setting for the sliding window of live Soroban state sizes, used in rent fee
///     calculations.
/// </summary>
public class ConfigSettingLiveSorobanStateSizeWindow : LedgerEntryConfigSetting
{
    private ConfigSettingLiveSorobanStateSizeWindow(ulong[] value)
    {
        InnerValue = value;
    }

    /// <summary>
    ///     The array of live Soroban state size snapshots used in rent fee calculations.
    /// </summary>
    public ulong[] InnerValue { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingLiveSorobanStateSizeWindow" /> from an XDR <c>Uint64[]</c> array.
    /// </summary>
    /// <param name="xdrConfig">The XDR array of Uint64 values.</param>
    /// <returns>A <see cref="ConfigSettingLiveSorobanStateSizeWindow" /> instance.</returns>
    public static ConfigSettingLiveSorobanStateSizeWindow FromXdr(Uint64[] xdrConfig)
    {
        return new ConfigSettingLiveSorobanStateSizeWindow(
            xdrConfig.Select(x => x.InnerValue).ToArray()
        );
    }
}