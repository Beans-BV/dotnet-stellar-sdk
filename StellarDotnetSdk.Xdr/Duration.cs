// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  typedef uint64 Duration;

//  ===========================================================================
public class Duration
{
    public Duration()
    {
    }

    public Duration(Uint64 value)
    {
        InnerValue = value;
    }

    public Uint64 InnerValue { get; set; }

    public static void Encode(XdrDataOutputStream stream, Duration encodedDuration)
    {
        Uint64.Encode(stream, encodedDuration.InnerValue);
    }

    public static Duration Decode(XdrDataInputStream stream)
    {
        var decodedDuration = new Duration();
        decodedDuration.InnerValue = Uint64.Decode(stream);
        return decodedDuration;
    }
}