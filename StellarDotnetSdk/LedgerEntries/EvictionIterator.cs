namespace StellarDotnetSdk.LedgerEntries;

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

    public uint BucketListLevel { get; }
    public bool IsCurrBucket { get; }
    public ulong BucketFileOffset { get; }

    public static EvictionIterator FromXdr(Xdr.EvictionIterator xdrConfig)
    {
        return new EvictionIterator(xdrConfig.BucketListLevel.InnerValue,
            xdrConfig.IsCurrBucket, xdrConfig.BucketFileOffset.InnerValue);
    }
}