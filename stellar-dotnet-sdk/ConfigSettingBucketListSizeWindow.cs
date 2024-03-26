using System.Collections.Generic;
using System.Linq;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingBucketListSizeWindow : LedgerEntryConfigSetting
{
    private ConfigSettingBucketListSizeWindow(IEnumerable<Uint64> value)
    {
        InnerValue = value.Select(x => x.InnerValue).ToArray();
    }

    public ulong[] InnerValue { get; }

    public static ConfigSettingBucketListSizeWindow FromXdr(Uint64[] xdrConfig)
    {
        return new ConfigSettingBucketListSizeWindow(xdrConfig);
    }
}