// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  typedef unsigned hyper uint64;

//  ===========================================================================
public class Uint64
{
    public Uint64()
    {
    }

    public Uint64(ulong value)
    {
        InnerValue = value;
    }

    public ulong InnerValue { get; set; }

    public static void Encode(XdrDataOutputStream stream, Uint64 encodedUint64)
    {
        stream.WriteULong(encodedUint64.InnerValue);
    }

    public static Uint64 Decode(XdrDataInputStream stream)
    {
        var decodedUint64 = new Uint64();
        decodedUint64.InnerValue = stream.ReadULong();
        return decodedUint64;
    }
}