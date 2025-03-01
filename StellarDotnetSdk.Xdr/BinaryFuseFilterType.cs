// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

using System;

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  enum BinaryFuseFilterType
//  {
//      BINARY_FUSE_FILTER_8_BIT = 0,
//      BINARY_FUSE_FILTER_16_BIT = 1,
//      BINARY_FUSE_FILTER_32_BIT = 2
//  };

//  ===========================================================================
public class BinaryFuseFilterType
{
    public enum BinaryFuseFilterTypeEnum
    {
        BINARY_FUSE_FILTER_8_BIT = 0,
        BINARY_FUSE_FILTER_16_BIT = 1,
        BINARY_FUSE_FILTER_32_BIT = 2,
    }

    public BinaryFuseFilterTypeEnum InnerValue { get; set; }

    public static BinaryFuseFilterType Create(BinaryFuseFilterTypeEnum v)
    {
        return new BinaryFuseFilterType
        {
            InnerValue = v,
        };
    }

    public static BinaryFuseFilterType Decode(XdrDataInputStream stream)
    {
        var value = stream.ReadInt();
        switch (value)
        {
            case 0: return Create(BinaryFuseFilterTypeEnum.BINARY_FUSE_FILTER_8_BIT);
            case 1: return Create(BinaryFuseFilterTypeEnum.BINARY_FUSE_FILTER_16_BIT);
            case 2: return Create(BinaryFuseFilterTypeEnum.BINARY_FUSE_FILTER_32_BIT);
            default:
                throw new Exception("Unknown enum value: " + value);
        }
    }

    public static void Encode(XdrDataOutputStream stream, BinaryFuseFilterType value)
    {
        stream.WriteInt((int)value.InnerValue);
    }
}