// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

using System;

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  enum HotArchiveBucketEntryType
//  {
//      HOT_ARCHIVE_METAENTRY = -1, // Bucket metadata, should come first.
//      HOT_ARCHIVE_ARCHIVED = 0,   // Entry is Archived
//      HOT_ARCHIVE_LIVE = 1,       // Entry was previously HOT_ARCHIVE_ARCHIVED, or HOT_ARCHIVE_DELETED, but
//                                  // has been added back to the live BucketList.
//                                  // Does not need to be persisted.
//      HOT_ARCHIVE_DELETED = 2     // Entry deleted (Note: must be persisted in archive)
//  };

//  ===========================================================================
public class HotArchiveBucketEntryType
{
    public enum HotArchiveBucketEntryTypeEnum
    {
        HOT_ARCHIVE_METAENTRY = -1,
        HOT_ARCHIVE_ARCHIVED = 0,
        HOT_ARCHIVE_LIVE = 1,
        HOT_ARCHIVE_DELETED = 2,
    }

    public HotArchiveBucketEntryTypeEnum InnerValue { get; set; }

    public static HotArchiveBucketEntryType Create(HotArchiveBucketEntryTypeEnum v)
    {
        return new HotArchiveBucketEntryType
        {
            InnerValue = v,
        };
    }

    public static HotArchiveBucketEntryType Decode(XdrDataInputStream stream)
    {
        var value = stream.ReadInt();
        switch (value)
        {
            case -1: return Create(HotArchiveBucketEntryTypeEnum.HOT_ARCHIVE_METAENTRY);
            case 0: return Create(HotArchiveBucketEntryTypeEnum.HOT_ARCHIVE_ARCHIVED);
            case 1: return Create(HotArchiveBucketEntryTypeEnum.HOT_ARCHIVE_LIVE);
            case 2: return Create(HotArchiveBucketEntryTypeEnum.HOT_ARCHIVE_DELETED);
            default:
                throw new Exception("Unknown enum value: " + value);
        }
    }

    public static void Encode(XdrDataOutputStream stream, HotArchiveBucketEntryType value)
    {
        stream.WriteInt((int)value.InnerValue);
    }
}