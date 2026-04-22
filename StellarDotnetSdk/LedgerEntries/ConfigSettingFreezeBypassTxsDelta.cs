using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the delta of transaction hashes being added to or removed from the freeze-bypass set
///     in a given ledger.
/// </summary>
public class ConfigSettingFreezeBypassTxsDelta : LedgerEntryConfigSetting
{
    private ConfigSettingFreezeBypassTxsDelta(byte[][] addTxs, byte[][] removeTxs)
    {
        AddTxs = addTxs;
        RemoveTxs = removeTxs;
    }

    /// <summary>
    ///     The list of transaction hashes being added to the freeze-bypass set.
    /// </summary>
    public byte[][] AddTxs { get; }

    /// <summary>
    ///     The list of transaction hashes being removed from the freeze-bypass set.
    /// </summary>
    public byte[][] RemoveTxs { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingFreezeBypassTxsDelta" /> from an XDR <see cref="FreezeBypassTxsDelta" /> object.
    /// </summary>
    /// <param name="xdrConfig">The XDR config setting object.</param>
    /// <returns>A <see cref="ConfigSettingFreezeBypassTxsDelta" /> instance.</returns>
    public static ConfigSettingFreezeBypassTxsDelta FromXdr(FreezeBypassTxsDelta xdrConfig)
    {
        return new ConfigSettingFreezeBypassTxsDelta(
            xdrConfig.AddTxs.Select(h => h.InnerValue).ToArray(),
            xdrConfig.RemoveTxs.Select(h => h.InnerValue).ToArray()
        );
    }
}
