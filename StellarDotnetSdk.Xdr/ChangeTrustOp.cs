// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct ChangeTrustOp
//  {
//      ChangeTrustAsset line;
//  
//      // if limit is set to 0, deletes the trust line
//      int64 limit;
//  };

//  ===========================================================================
public class ChangeTrustOp
{
    public ChangeTrustAsset Line { get; set; }
    public Int64 Limit { get; set; }

    public static void Encode(XdrDataOutputStream stream, ChangeTrustOp encodedChangeTrustOp)
    {
        ChangeTrustAsset.Encode(stream, encodedChangeTrustOp.Line);
        Int64.Encode(stream, encodedChangeTrustOp.Limit);
    }

    public static ChangeTrustOp Decode(XdrDataInputStream stream)
    {
        var decodedChangeTrustOp = new ChangeTrustOp();
        decodedChangeTrustOp.Line = ChangeTrustAsset.Decode(stream);
        decodedChangeTrustOp.Limit = Int64.Decode(stream);
        return decodedChangeTrustOp;
    }
}