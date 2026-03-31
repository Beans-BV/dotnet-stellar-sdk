namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration settings for Soroban state archival, including TTL limits, rent rates, and
///     eviction parameters.
/// </summary>
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

    /// <summary>
    ///     Maximum time-to-live (TTL) in ledgers for any ledger entry.
    /// </summary>
    public uint MaxEntryTtl { get; }

    /// <summary>
    ///     Minimum TTL in ledgers for temporary ledger entries.
    /// </summary>
    public uint MinTemporaryTtl { get; }

    /// <summary>
    ///     Minimum TTL in ledgers for persistent ledger entries.
    /// </summary>
    public uint MinPersistentTtl { get; }

    /// <summary>
    ///     Denominator for computing the persistent entry rent fee. Rent fee equals
    ///     <c>writeFeeRateAverage / persistentRentRateDenominator</c>.
    /// </summary>
    public long PersistentRentRateDenominator { get; }

    /// <summary>
    ///     Denominator for computing the temporary entry rent fee. Rent fee equals
    ///     <c>writeFeeRateAverage / tempRentRateDenominator</c>.
    /// </summary>
    public long TempRentRateDenominator { get; }

    /// <summary>
    ///     Maximum number of entries that emit archival meta in a single ledger.
    /// </summary>
    public uint MaxEntriesToArchive { get; }

    /// <summary>
    ///     Number of snapshots to use when calculating average live Soroban state size.
    /// </summary>
    public uint LiveSorobanStateSizeWindowSampleSize { get; }

    /// <summary>
    ///     How often to sample the live Soroban state size for the average, in ledgers.
    /// </summary>
    public uint LiveSorobanStateSizeWindowSamplePeriod { get; }

    /// <summary>
    ///     Maximum number of bytes that are scanned for eviction per ledger.
    /// </summary>
    public uint EvictionScanSize { get; }

    /// <summary>
    ///     Lowest BucketList level to be scanned to evict entries.
    /// </summary>
    public uint StartingEvictionScanLevel { get; }

    /// <summary>
    ///     Creates a <see cref="StateArchivalSettings" /> from an XDR <see cref="Xdr.StateArchivalSettings" /> object.
    /// </summary>
    /// <param name="xdrConfig">The XDR config setting object.</param>
    /// <returns>A <see cref="StateArchivalSettings" /> instance.</returns>
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