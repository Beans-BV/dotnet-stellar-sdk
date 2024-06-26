// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct SCPBallot
//  {
//      uint32 counter; // n
//      Value value;    // x
//  };

//  ===========================================================================
public class SCPBallot
{
    public Uint32 Counter { get; set; }
    public Value Value { get; set; }

    public static void Encode(XdrDataOutputStream stream, SCPBallot encodedSCPBallot)
    {
        Uint32.Encode(stream, encodedSCPBallot.Counter);
        Value.Encode(stream, encodedSCPBallot.Value);
    }

    public static SCPBallot Decode(XdrDataInputStream stream)
    {
        var decodedSCPBallot = new SCPBallot();
        decodedSCPBallot.Counter = Uint32.Decode(stream);
        decodedSCPBallot.Value = Value.Decode(stream);
        return decodedSCPBallot;
    }
}