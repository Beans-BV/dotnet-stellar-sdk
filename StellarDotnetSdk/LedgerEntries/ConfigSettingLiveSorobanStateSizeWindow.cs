using System.Collections.Generic;
using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

public class ConfigSettingLiveSorobanStateSizeWindow : LedgerEntryConfigSetting
{
    private ConfigSettingLiveSorobanStateSizeWindow(ulong[] value)
    {
        InnerValue = value;
    }

    public ulong[] InnerValue { get; }

    public static ConfigSettingLiveSorobanStateSizeWindow FromXdr(Uint64[] xdrConfig)
    {
        return new ConfigSettingLiveSorobanStateSizeWindow(
            xdrConfig.Select(x => x.InnerValue).ToArray()
        );
    }
}