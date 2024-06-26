// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct TTLEntry {
//      // Hash of the LedgerKey that is associated with this TTLEntry
//      Hash keyHash;
//      uint32 liveUntilLedgerSeq;
//  };

//  ===========================================================================
public class TTLEntry
{
    public Hash KeyHash { get; set; }
    public Uint32 LiveUntilLedgerSeq { get; set; }

    public static void Encode(XdrDataOutputStream stream, TTLEntry encodedTTLEntry)
    {
        Hash.Encode(stream, encodedTTLEntry.KeyHash);
        Uint32.Encode(stream, encodedTTLEntry.LiveUntilLedgerSeq);
    }

    public static TTLEntry Decode(XdrDataInputStream stream)
    {
        var decodedTTLEntry = new TTLEntry();
        decodedTTLEntry.KeyHash = Hash.Decode(stream);
        decodedTTLEntry.LiveUntilLedgerSeq = Uint32.Decode(stream);
        return decodedTTLEntry;
    }
}