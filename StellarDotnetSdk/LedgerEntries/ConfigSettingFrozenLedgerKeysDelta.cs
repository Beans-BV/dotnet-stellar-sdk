using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the delta of ledger keys being frozen or unfrozen in a given ledger.
/// </summary>
public class ConfigSettingFrozenLedgerKeysDelta : LedgerEntryConfigSetting
{
    private ConfigSettingFrozenLedgerKeysDelta(byte[][] keysToFreeze, byte[][] keysToUnfreeze)
    {
        KeysToFreeze = keysToFreeze;
        KeysToUnfreeze = keysToUnfreeze;
    }

    /// <summary>
    ///     The list of encoded ledger keys that will be frozen.
    /// </summary>
    public byte[][] KeysToFreeze { get; }

    /// <summary>
    ///     The list of encoded ledger keys that will be unfrozen.
    /// </summary>
    public byte[][] KeysToUnfreeze { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingFrozenLedgerKeysDelta" /> from an XDR <see cref="FrozenLedgerKeysDelta" /> object.
    /// </summary>
    /// <param name="xdrConfig">The XDR config setting object.</param>
    /// <returns>A <see cref="ConfigSettingFrozenLedgerKeysDelta" /> instance.</returns>
    public static ConfigSettingFrozenLedgerKeysDelta FromXdr(FrozenLedgerKeysDelta xdrConfig)
    {
        return new ConfigSettingFrozenLedgerKeysDelta(
            xdrConfig.KeysToFreeze.Select(k => k.InnerValue).ToArray(),
            xdrConfig.KeysToUnfreeze.Select(k => k.InnerValue).ToArray()
        );
    }
}
