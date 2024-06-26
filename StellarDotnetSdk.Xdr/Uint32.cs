// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  typedef unsigned int uint32;

//  ===========================================================================
public class Uint32
{
    public Uint32()
    {
    }

    public Uint32(uint value)
    {
        InnerValue = value;
    }

    public uint InnerValue { get; set; }

    public static void Encode(XdrDataOutputStream stream, Uint32 encodedUint32)
    {
        stream.WriteUInt(encodedUint32.InnerValue);
    }

    public static Uint32 Decode(XdrDataInputStream stream)
    {
        var decodedUint32 = new Uint32();
        decodedUint32.InnerValue = stream.ReadUInt();
        return decodedUint32;
    }
}