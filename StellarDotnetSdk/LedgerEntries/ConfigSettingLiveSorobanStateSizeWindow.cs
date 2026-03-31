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

    public ulong[] InnerValue { get; }

    public static ConfigSettingLiveSorobanStateSizeWindow FromXdr(Uint64[] xdrConfig)
    {
        return new ConfigSettingLiveSorobanStateSizeWindow(
            xdrConfig.Select(x => x.InnerValue).ToArray()
        );
    }
}