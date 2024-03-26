namespace stellar_dotnet_sdk;

public class StateArchivalSettings : LedgerEntryConfigSetting
{
    private StateArchivalSettings(uint maxEntryTTL, uint minTemporaryTTL, uint minPersistentTTL,
        long persistentRentRateDenominator, long tempRentRateDenominator, uint maxEntriesToArchive,
        uint bucketListSizeWindowSampleSize, uint bucketListWindowSamplePeriod, uint evictionScanSize,
        uint startingEvictionScanLevel)
    {
        MaxEntryTTL = maxEntryTTL;
        MinTemporaryTTL = minTemporaryTTL;
        MinPersistentTTL = minPersistentTTL;
        PersistentRentRateDenominator = persistentRentRateDenominator;
        TempRentRateDenominator = tempRentRateDenominator;
        MaxEntriesToArchive = maxEntriesToArchive;
        BucketListSizeWindowSampleSize = bucketListSizeWindowSampleSize;
        BucketListWindowSamplePeriod = bucketListWindowSamplePeriod;
        EvictionScanSize = evictionScanSize;
        StartingEvictionScanLevel = startingEvictionScanLevel;
    }

    public uint MaxEntryTTL { get; }
    public uint MinTemporaryTTL { get; }
    public uint MinPersistentTTL { get; }
    public long PersistentRentRateDenominator { get; }
    public long TempRentRateDenominator { get; }
    public uint MaxEntriesToArchive { get; }
    public uint BucketListSizeWindowSampleSize { get; }
    public uint BucketListWindowSamplePeriod { get; }
    public uint EvictionScanSize { get; }
    public uint StartingEvictionScanLevel { get; }

    public static StateArchivalSettings FromXdr(xdr.StateArchivalSettings xdrConfig)
    {
        return new StateArchivalSettings(xdrConfig.MaxEntryTTL.InnerValue,
            xdrConfig.MinTemporaryTTL.InnerValue,
            xdrConfig.MinPersistentTTL.InnerValue,
            xdrConfig.PersistentRentRateDenominator.InnerValue,
            xdrConfig.TempRentRateDenominator.InnerValue,
            xdrConfig.MaxEntriesToArchive.InnerValue,
            xdrConfig.BucketListSizeWindowSampleSize.InnerValue,
            xdrConfig.BucketListWindowSamplePeriod.InnerValue,
            xdrConfig.EvictionScanSize.InnerValue,
            xdrConfig.StartingEvictionScanLevel.InnerValue);
    }
}