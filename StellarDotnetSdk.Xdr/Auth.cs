// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct Auth
//  {
//      int flags;
//  };

//  ===========================================================================
public class Auth
{
    public int Flags { get; set; }

    public static void Encode(XdrDataOutputStream stream, Auth encodedAuth)
    {
        stream.WriteInt(encodedAuth.Flags);
    }

    public static Auth Decode(XdrDataInputStream stream)
    {
        var decodedAuth = new Auth();
        decodedAuth.Flags = stream.ReadInt();
        return decodedAuth;
    }
}