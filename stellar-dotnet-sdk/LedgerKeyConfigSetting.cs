using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerKeyConfigSetting : LedgerKey
{
    public LedgerKeyConfigSetting(ConfigSettingID settingId)
    {
        ConfigSettingID = settingId;
    }

    public ConfigSettingID ConfigSettingID { get; }

    public override xdr.LedgerKey ToXdr()
    {
        return new xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING },
            ConfigSetting = new xdr.LedgerKey.LedgerKeyConfigSetting
            {
                ConfigSettingID = ConfigSettingID
            }
        };
    }

    public static LedgerKeyConfigSetting FromXdr(xdr.LedgerKey.LedgerKeyConfigSetting xdr)
    {
        return new LedgerKeyConfigSetting(xdr.ConfigSettingID);
    }
}