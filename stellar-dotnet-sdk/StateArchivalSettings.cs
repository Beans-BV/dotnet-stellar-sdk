using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class StateArchivalSettings : LedgerEntryConfigSetting
{
    public uint MaxEntryTTL { get; set; }
    public uint MinTemporaryTTL { get; set; }
    public uint MinPersistentTTL { get; set; }
    public long PersistentRentRateDenominator { get; set; }
    public long TempRentRateDenominator { get; set; }
    public uint MaxEntriesToArchive { get; set; }
    public uint BucketListSizeWindowSampleSize { get; set; }
    public ulong EvictionScanSize { get; set; }
    public uint StartingEvictionScanLevel { get; set; }

    public static StateArchivalSettings FromXdr(xdr.StateArchivalSettings xdrConfig)
    {
        return new StateArchivalSettings
        {
            MaxEntryTTL = xdrConfig.MaxEntryTTL.InnerValue,
            MinTemporaryTTL = xdrConfig.MinTemporaryTTL.InnerValue,
            MinPersistentTTL = xdrConfig.MinPersistentTTL.InnerValue,
            PersistentRentRateDenominator = xdrConfig.PersistentRentRateDenominator.InnerValue,
            TempRentRateDenominator = xdrConfig.TempRentRateDenominator.InnerValue,
            MaxEntriesToArchive = xdrConfig.MaxEntriesToArchive.InnerValue,
            BucketListSizeWindowSampleSize = xdrConfig.BucketListSizeWindowSampleSize.InnerValue,
            EvictionScanSize = xdrConfig.EvictionScanSize.InnerValue,
            StartingEvictionScanLevel = xdrConfig.StartingEvictionScanLevel.InnerValue
        };
    }

    public xdr.StateArchivalSettings ToXdr()
    {
        return new xdr.StateArchivalSettings
        {
            MaxEntryTTL = new Uint32(MaxEntryTTL),
            MinTemporaryTTL = new Uint32(MinTemporaryTTL),
            MinPersistentTTL = new Uint32(MinPersistentTTL),
            PersistentRentRateDenominator = new Int64(PersistentRentRateDenominator),
            TempRentRateDenominator = new Int64(TempRentRateDenominator),
            MaxEntriesToArchive = new Uint32(MaxEntriesToArchive),
            BucketListSizeWindowSampleSize = new Uint32(BucketListSizeWindowSampleSize),
            EvictionScanSize = new Uint64(EvictionScanSize),
            StartingEvictionScanLevel = new Uint32(StartingEvictionScanLevel)
        };
    }

    public ConfigSettingEntry ToXdrConfigSettingEntry()
    {
        return new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_STATE_ARCHIVAL),
            StateArchivalSettings = ToXdr()
        };
    }
}