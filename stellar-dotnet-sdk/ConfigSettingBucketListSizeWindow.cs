using System.Collections.Generic;
using System.Linq;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingBucketListSizeWindow : LedgerEntryConfigSetting
{
    public ConfigSettingBucketListSizeWindow(IEnumerable<Uint64> value)
    {
        InnerValue = value.Select(x => x.InnerValue).ToArray();
    }

    public ConfigSettingBucketListSizeWindow(ulong[] value)
    {
        InnerValue = value;
    }

    public ulong[] InnerValue { get; }

    public ConfigSettingEntry ToXdrConfigSettingEntry()
    {
        return new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_BUCKETLIST_SIZE_WINDOW),
            BucketListSizeWindow = InnerValue.Select(x => new Uint64(x)).ToArray()
        };
    }
}