using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration setting storing the set of transaction hashes that bypass
///     the frozen-ledger-key enforcement.
/// </summary>
public class ConfigSettingFreezeBypassTxs : LedgerEntryConfigSetting
{
    private ConfigSettingFreezeBypassTxs(byte[][] txHashes)
    {
        TxHashes = txHashes;
    }

    /// <summary>
    ///     The list of transaction hashes allowed to access frozen ledger keys.
    /// </summary>
    public byte[][] TxHashes { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingFreezeBypassTxs" /> from an XDR <see cref="FreezeBypassTxs" /> object.
    /// </summary>
    /// <param name="xdrConfig">The XDR config setting object.</param>
    /// <returns>A <see cref="ConfigSettingFreezeBypassTxs" /> instance.</returns>
    public static ConfigSettingFreezeBypassTxs FromXdr(FreezeBypassTxs xdrConfig)
    {
        return new ConfigSettingFreezeBypassTxs(
            xdrConfig.TxHashes.Select(h => h.InnerValue).ToArray()
        );
    }
}
