// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  typedef uint64 TimePoint;

//  ===========================================================================
public class TimePoint
{
    public TimePoint()
    {
    }

    public TimePoint(Uint64 value)
    {
        InnerValue = value;
    }

    public Uint64 InnerValue { get; set; }

    public static void Encode(XdrDataOutputStream stream, TimePoint encodedTimePoint)
    {
        Uint64.Encode(stream, encodedTimePoint.InnerValue);
    }

    public static TimePoint Decode(XdrDataInputStream stream)
    {
        var decodedTimePoint = new TimePoint();
        decodedTimePoint.InnerValue = Uint64.Decode(stream);
        return decodedTimePoint;
    }
}