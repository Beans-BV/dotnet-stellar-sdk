// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

using System;

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  enum IPAddrType
//  {
//      IPv4 = 0,
//      IPv6 = 1
//  };

//  ===========================================================================
public class IPAddrType
{
    public enum IPAddrTypeEnum
    {
        IPv4 = 0,
        IPv6 = 1
    }

    public IPAddrTypeEnum InnerValue { get; set; }

    public static IPAddrType Create(IPAddrTypeEnum v)
    {
        return new IPAddrType
        {
            InnerValue = v
        };
    }

    public static IPAddrType Decode(XdrDataInputStream stream)
    {
        var value = stream.ReadInt();
        switch (value)
        {
            case 0: return Create(IPAddrTypeEnum.IPv4);
            case 1: return Create(IPAddrTypeEnum.IPv6);
            default:
                throw new Exception("Unknown enum value: " + value);
        }
    }

    public static void Encode(XdrDataOutputStream stream, IPAddrType value)
    {
        stream.WriteInt((int)value.InnerValue);
    }
}