using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class EvictionIterator : LedgerEntryConfigSetting
{
    public uint BucketListLevel { get; set; }
    public bool IsCurrBucket { get; set; }
    public ulong BucketFileOffset { get; set; }

    public static EvictionIterator FromXdr(xdr.EvictionIterator xdrConfig)
    {
        return new EvictionIterator
        {
            BucketListLevel = xdrConfig.BucketListLevel.InnerValue,
            IsCurrBucket = xdrConfig.IsCurrBucket,
            BucketFileOffset = xdrConfig.BucketFileOffset.InnerValue
        };
    }

    public xdr.EvictionIterator ToXdr()
    {
        return new xdr.EvictionIterator
        {
            BucketListLevel = new Uint32(BucketListLevel),
            IsCurrBucket = IsCurrBucket,
            BucketFileOffset = new Uint64(BucketFileOffset)
        };
    }

    public ConfigSettingEntry ToXdrConfigSettingEntry()
    {
        return new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_EVICTION_ITERATOR),
            EvictionIterator = ToXdr()
        };
    }
}