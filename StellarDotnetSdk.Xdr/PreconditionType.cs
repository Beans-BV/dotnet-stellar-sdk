// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

using System;

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  enum PreconditionType
//  {
//      PRECOND_NONE = 0,
//      PRECOND_TIME = 1,
//      PRECOND_V2 = 2
//  };

//  ===========================================================================
public class PreconditionType
{
    public enum PreconditionTypeEnum
    {
        PRECOND_NONE = 0,
        PRECOND_TIME = 1,
        PRECOND_V2 = 2
    }

    public PreconditionTypeEnum InnerValue { get; set; }

    public static PreconditionType Create(PreconditionTypeEnum v)
    {
        return new PreconditionType
        {
            InnerValue = v
        };
    }

    public static PreconditionType Decode(XdrDataInputStream stream)
    {
        var value = stream.ReadInt();
        switch (value)
        {
            case 0: return Create(PreconditionTypeEnum.PRECOND_NONE);
            case 1: return Create(PreconditionTypeEnum.PRECOND_TIME);
            case 2: return Create(PreconditionTypeEnum.PRECOND_V2);
            default:
                throw new Exception("Unknown enum value: " + value);
        }
    }

    public static void Encode(XdrDataOutputStream stream, PreconditionType value)
    {
        stream.WriteInt((int)value.InnerValue);
    }
}