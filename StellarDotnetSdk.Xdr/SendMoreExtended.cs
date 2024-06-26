// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct SendMoreExtended
//  {
//      uint32 numMessages;
//      uint32 numBytes;
//  };

//  ===========================================================================
public class SendMoreExtended
{
    public Uint32 NumMessages { get; set; }
    public Uint32 NumBytes { get; set; }

    public static void Encode(XdrDataOutputStream stream, SendMoreExtended encodedSendMoreExtended)
    {
        Uint32.Encode(stream, encodedSendMoreExtended.NumMessages);
        Uint32.Encode(stream, encodedSendMoreExtended.NumBytes);
    }

    public static SendMoreExtended Decode(XdrDataInputStream stream)
    {
        var decodedSendMoreExtended = new SendMoreExtended();
        decodedSendMoreExtended.NumMessages = Uint32.Decode(stream);
        decodedSendMoreExtended.NumBytes = Uint32.Decode(stream);
        return decodedSendMoreExtended;
    }
}