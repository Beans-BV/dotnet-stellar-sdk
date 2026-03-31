using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

/// <summary>
///     Represents a ledger key for a network configuration setting entry.
///     Used to look up protocol-level configuration values from the ledger.
/// </summary>
public class LedgerKeyConfigSetting : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyConfigSetting</c> from a configuration setting identifier.
    /// </summary>
    /// <param name="settingId">The configuration setting identifier.</param>
    public LedgerKeyConfigSetting(ConfigSettingID settingId)
    {
        ConfigSettingId = settingId;
    }

    /// <summary>
    ///     The configuration setting identifier.
    /// </summary>
    public ConfigSettingID ConfigSettingId { get; }

    /// <summary>
    ///     Serializes this ledger key to its XDR representation.
    /// </summary>
    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING },
            ConfigSetting = new Xdr.LedgerKey.LedgerKeyConfigSetting
            {
                ConfigSettingID = ConfigSettingId,
            },
        };
    }

    /// <summary>
    ///     Deserializes a <see cref="LedgerKeyConfigSetting" /> from its XDR representation.
    /// </summary>
    /// <param name="xdr">The XDR ledger key config setting object.</param>
    public static LedgerKeyConfigSetting FromXdr(Xdr.LedgerKey.LedgerKeyConfigSetting xdr)
    {
        return new LedgerKeyConfigSetting(xdr.ConfigSettingID);
    }
}