using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration setting storing the set of currently frozen ledger keys.
/// </summary>
public class ConfigSettingFrozenLedgerKeys : LedgerEntryConfigSetting
{
    private ConfigSettingFrozenLedgerKeys(byte[][] keys)
    {
        Keys = keys;
    }

    /// <summary>
    ///     The list of encoded ledger keys that are currently frozen.
    /// </summary>
    public byte[][] Keys { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingFrozenLedgerKeys" /> from an XDR <see cref="FrozenLedgerKeys" /> object.
    /// </summary>
    /// <param name="xdrConfig">The XDR config setting object.</param>
    /// <returns>A <see cref="ConfigSettingFrozenLedgerKeys" /> instance.</returns>
    public static ConfigSettingFrozenLedgerKeys FromXdr(FrozenLedgerKeys xdrConfig)
    {
        return new ConfigSettingFrozenLedgerKeys(
            xdrConfig.Keys.Select(k => k.InnerValue).ToArray()
        );
    }
}
