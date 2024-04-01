using System.Collections.Generic;
using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

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