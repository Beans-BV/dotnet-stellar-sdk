using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

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