// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct SetTrustLineFlagsOp
//  {
//      AccountID trustor;
//      Asset asset;
//  
//      uint32 clearFlags; // which flags to clear
//      uint32 setFlags;   // which flags to set
//  };

//  ===========================================================================
public class SetTrustLineFlagsOp
{
    public AccountID Trustor { get; set; }
    public Asset Asset { get; set; }
    public Uint32 ClearFlags { get; set; }
    public Uint32 SetFlags { get; set; }

    public static void Encode(XdrDataOutputStream stream, SetTrustLineFlagsOp encodedSetTrustLineFlagsOp)
    {
        AccountID.Encode(stream, encodedSetTrustLineFlagsOp.Trustor);
        Asset.Encode(stream, encodedSetTrustLineFlagsOp.Asset);
        Uint32.Encode(stream, encodedSetTrustLineFlagsOp.ClearFlags);
        Uint32.Encode(stream, encodedSetTrustLineFlagsOp.SetFlags);
    }

    public static SetTrustLineFlagsOp Decode(XdrDataInputStream stream)
    {
        var decodedSetTrustLineFlagsOp = new SetTrustLineFlagsOp();
        decodedSetTrustLineFlagsOp.Trustor = AccountID.Decode(stream);
        decodedSetTrustLineFlagsOp.Asset = Asset.Decode(stream);
        decodedSetTrustLineFlagsOp.ClearFlags = Uint32.Decode(stream);
        decodedSetTrustLineFlagsOp.SetFlags = Uint32.Decode(stream);
        return decodedSetTrustLineFlagsOp;
    }
}