// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace stellar_dotnet_sdk.xdr;

// === xdr source ============================================================

//  struct FloodDemand
//  {
//      TxDemandVector txHashes;
//  };

//  ===========================================================================
public class FloodDemand
{
    public TxDemandVector TxHashes { get; set; }

    public static void Encode(XdrDataOutputStream stream, FloodDemand encodedFloodDemand)
    {
        TxDemandVector.Encode(stream, encodedFloodDemand.TxHashes);
    }

    public static FloodDemand Decode(XdrDataInputStream stream)
    {
        var decodedFloodDemand = new FloodDemand();
        decodedFloodDemand.TxHashes = TxDemandVector.Decode(stream);
        return decodedFloodDemand;
    }
}