// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct TimeSlicedNodeData
//  {
//      uint32 addedAuthenticatedPeers;
//      uint32 droppedAuthenticatedPeers;
//      uint32 totalInboundPeerCount;
//      uint32 totalOutboundPeerCount;
//  
//      // SCP stats
//      uint32 p75SCPFirstToSelfLatencyMs;
//      uint32 p75SCPSelfToOtherLatencyMs;
//  
//      // How many times the node lost sync in the time slice
//      uint32 lostSyncCount;
//  
//      // Config data
//      bool isValidator;
//      uint32 maxInboundPeerCount;
//      uint32 maxOutboundPeerCount;
//  };

//  ===========================================================================
public class TimeSlicedNodeData
{
    public Uint32 AddedAuthenticatedPeers { get; set; }
    public Uint32 DroppedAuthenticatedPeers { get; set; }
    public Uint32 TotalInboundPeerCount { get; set; }
    public Uint32 TotalOutboundPeerCount { get; set; }
    public Uint32 P75SCPFirstToSelfLatencyMs { get; set; }
    public Uint32 P75SCPSelfToOtherLatencyMs { get; set; }
    public Uint32 LostSyncCount { get; set; }
    public bool IsValidator { get; set; }
    public Uint32 MaxInboundPeerCount { get; set; }
    public Uint32 MaxOutboundPeerCount { get; set; }

    public static void Encode(XdrDataOutputStream stream, TimeSlicedNodeData encodedTimeSlicedNodeData)
    {
        Uint32.Encode(stream, encodedTimeSlicedNodeData.AddedAuthenticatedPeers);
        Uint32.Encode(stream, encodedTimeSlicedNodeData.DroppedAuthenticatedPeers);
        Uint32.Encode(stream, encodedTimeSlicedNodeData.TotalInboundPeerCount);
        Uint32.Encode(stream, encodedTimeSlicedNodeData.TotalOutboundPeerCount);
        Uint32.Encode(stream, encodedTimeSlicedNodeData.P75SCPFirstToSelfLatencyMs);
        Uint32.Encode(stream, encodedTimeSlicedNodeData.P75SCPSelfToOtherLatencyMs);
        Uint32.Encode(stream, encodedTimeSlicedNodeData.LostSyncCount);
        stream.WriteInt(encodedTimeSlicedNodeData.IsValidator ? 1 : 0);
        Uint32.Encode(stream, encodedTimeSlicedNodeData.MaxInboundPeerCount);
        Uint32.Encode(stream, encodedTimeSlicedNodeData.MaxOutboundPeerCount);
    }

    public static TimeSlicedNodeData Decode(XdrDataInputStream stream)
    {
        var decodedTimeSlicedNodeData = new TimeSlicedNodeData();
        decodedTimeSlicedNodeData.AddedAuthenticatedPeers = Uint32.Decode(stream);
        decodedTimeSlicedNodeData.DroppedAuthenticatedPeers = Uint32.Decode(stream);
        decodedTimeSlicedNodeData.TotalInboundPeerCount = Uint32.Decode(stream);
        decodedTimeSlicedNodeData.TotalOutboundPeerCount = Uint32.Decode(stream);
        decodedTimeSlicedNodeData.P75SCPFirstToSelfLatencyMs = Uint32.Decode(stream);
        decodedTimeSlicedNodeData.P75SCPSelfToOtherLatencyMs = Uint32.Decode(stream);
        decodedTimeSlicedNodeData.LostSyncCount = Uint32.Decode(stream);
        decodedTimeSlicedNodeData.IsValidator = stream.ReadInt() == 1 ? true : false;
        decodedTimeSlicedNodeData.MaxInboundPeerCount = Uint32.Decode(stream);
        decodedTimeSlicedNodeData.MaxOutboundPeerCount = Uint32.Decode(stream);
        return decodedTimeSlicedNodeData;
    }
}