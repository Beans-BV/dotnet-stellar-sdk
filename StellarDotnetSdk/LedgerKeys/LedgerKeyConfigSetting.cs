using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

/// <summary>
///     Represents a ledger key for a network configuration setting entry.
///     Used to look up protocol-level configuration values from the ledger.
/// </summary>
public class LedgerKeyConfigSetting : LedgerKey
{
    public LedgerKeyConfigSetting(ConfigSettingID settingId)
    {
        ConfigSettingId = settingId;
    }

    public ConfigSettingID ConfigSettingId { get; }

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

    public static LedgerKeyConfigSetting FromXdr(Xdr.LedgerKey.LedgerKeyConfigSetting xdr)
    {
        return new LedgerKeyConfigSetting(xdr.ConfigSettingID);
    }
}