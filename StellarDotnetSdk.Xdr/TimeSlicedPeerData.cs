// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct TimeSlicedPeerData
//  {
//      PeerStats peerStats;
//      uint32 averageLatencyMs;
//  };

//  ===========================================================================
public class TimeSlicedPeerData
{
    public PeerStats PeerStats { get; set; }
    public Uint32 AverageLatencyMs { get; set; }

    public static void Encode(XdrDataOutputStream stream, TimeSlicedPeerData encodedTimeSlicedPeerData)
    {
        PeerStats.Encode(stream, encodedTimeSlicedPeerData.PeerStats);
        Uint32.Encode(stream, encodedTimeSlicedPeerData.AverageLatencyMs);
    }

    public static TimeSlicedPeerData Decode(XdrDataInputStream stream)
    {
        var decodedTimeSlicedPeerData = new TimeSlicedPeerData();
        decodedTimeSlicedPeerData.PeerStats = PeerStats.Decode(stream);
        decodedTimeSlicedPeerData.AverageLatencyMs = Uint32.Decode(stream);
        return decodedTimeSlicedPeerData;
    }
}