// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct SendMore
//  {
//      uint32 numMessages;
//  };

//  ===========================================================================
public class SendMore
{
    public Uint32 NumMessages { get; set; }

    public static void Encode(XdrDataOutputStream stream, SendMore encodedSendMore)
    {
        Uint32.Encode(stream, encodedSendMore.NumMessages);
    }

    public static SendMore Decode(XdrDataInputStream stream)
    {
        var decodedSendMore = new SendMore();
        decodedSendMore.NumMessages = Uint32.Decode(stream);
        return decodedSendMore;
    }
}