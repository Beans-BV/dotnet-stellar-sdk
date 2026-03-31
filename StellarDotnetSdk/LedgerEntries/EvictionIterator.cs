namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the eviction iterator state used to track progress of Soroban state entry eviction across ledger closes.
/// </summary>
public class EvictionIterator : LedgerEntryConfigSetting
{
    private EvictionIterator(
        uint bucketListLevel,
        bool isCurrBucket,
        ulong bucketFileOffset
    )
    {
        BucketListLevel = bucketListLevel;
        IsCurrBucket = isCurrBucket;
        BucketFileOffset = bucketFileOffset;
    }

    /// <summary>
    ///     The BucketList level currently being scanned for eviction.
    /// </summary>
    public uint BucketListLevel { get; }

    /// <summary>
    ///     Whether the iterator is pointing at the current bucket (<c>true</c>) or the snap bucket (<c>false</c>).
    /// </summary>
    public bool IsCurrBucket { get; }

    /// <summary>
    ///     The byte offset within the bucket file where the scan is currently positioned.
    /// </summary>
    public ulong BucketFileOffset { get; }

    /// <summary>
    ///     Creates an <see cref="EvictionIterator" /> from an XDR <see cref="Xdr.EvictionIterator" /> object.
    /// </summary>
    /// <param name="xdrConfig">The XDR eviction iterator object.</param>
    /// <returns>An <see cref="EvictionIterator" /> instance.</returns>
    public static EvictionIterator FromXdr(Xdr.EvictionIterator xdrConfig)
    {
        return new EvictionIterator(xdrConfig.BucketListLevel.InnerValue,
            xdrConfig.IsCurrBucket, xdrConfig.BucketFileOffset.InnerValue);
    }
}