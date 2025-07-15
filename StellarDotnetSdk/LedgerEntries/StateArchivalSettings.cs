namespace StellarDotnetSdk.LedgerEntries;

public class StateArchivalSettings : LedgerEntryConfigSetting
{
    private StateArchivalSettings(
        uint maxEntryTtl,
        uint minTemporaryTtl,
        uint minPersistentTtl,
        long persistentRentRateDenominator,
        long tempRentRateDenominator,
        uint maxEntriesToArchive,
        uint liveSorobanStateSizeWindowSampleSize,
        uint liveSorobanStateSizeWindowSamplePeriod,
        uint evictionScanSize,
        uint startingEvictionScanLevel)
    {
        MaxEntryTtl = maxEntryTtl;
        MinTemporaryTtl = minTemporaryTtl;
        MinPersistentTtl = minPersistentTtl;
        PersistentRentRateDenominator = persistentRentRateDenominator;
        TempRentRateDenominator = tempRentRateDenominator;
        MaxEntriesToArchive = maxEntriesToArchive;
        LiveSorobanStateSizeWindowSampleSize = liveSorobanStateSizeWindowSampleSize;
        LiveSorobanStateSizeWindowSamplePeriod = liveSorobanStateSizeWindowSamplePeriod;
        EvictionScanSize = evictionScanSize;
        StartingEvictionScanLevel = startingEvictionScanLevel;
    }

    public uint MaxEntryTtl { get; }
    public uint MinTemporaryTtl { get; }
    public uint MinPersistentTtl { get; }
    public long PersistentRentRateDenominator { get; }
    public long TempRentRateDenominator { get; }
    public uint MaxEntriesToArchive { get; }
    public uint LiveSorobanStateSizeWindowSampleSize { get; }
    public uint LiveSorobanStateSizeWindowSamplePeriod { get; }
    public uint EvictionScanSize { get; }
    public uint StartingEvictionScanLevel { get; }

    public static StateArchivalSettings FromXdr(Xdr.StateArchivalSettings xdrConfig)
    {
        return new StateArchivalSettings(
            xdrConfig.MaxEntryTTL.InnerValue,
            xdrConfig.MinTemporaryTTL.InnerValue,
            xdrConfig.MinPersistentTTL.InnerValue,
            xdrConfig.PersistentRentRateDenominator.InnerValue,
            xdrConfig.TempRentRateDenominator.InnerValue,
            xdrConfig.MaxEntriesToArchive.InnerValue,
            xdrConfig.LiveSorobanStateSizeWindowSampleSize.InnerValue,
            xdrConfig.LiveSorobanStateSizeWindowSamplePeriod.InnerValue,
            xdrConfig.EvictionScanSize.InnerValue,
            xdrConfig.StartingEvictionScanLevel.InnerValue
        );
    }
}