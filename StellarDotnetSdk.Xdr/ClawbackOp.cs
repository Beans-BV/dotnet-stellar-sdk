// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct ClawbackOp
//  {
//      Asset asset;
//      MuxedAccount from;
//      int64 amount;
//  };

//  ===========================================================================
public class ClawbackOp
{
    public Asset Asset { get; set; }
    public MuxedAccount From { get; set; }
    public Int64 Amount { get; set; }

    public static void Encode(XdrDataOutputStream stream, ClawbackOp encodedClawbackOp)
    {
        Asset.Encode(stream, encodedClawbackOp.Asset);
        MuxedAccount.Encode(stream, encodedClawbackOp.From);
        Int64.Encode(stream, encodedClawbackOp.Amount);
    }

    public static ClawbackOp Decode(XdrDataInputStream stream)
    {
        var decodedClawbackOp = new ClawbackOp();
        decodedClawbackOp.Asset = Asset.Decode(stream);
        decodedClawbackOp.From = MuxedAccount.Decode(stream);
        decodedClawbackOp.Amount = Int64.Decode(stream);
        return decodedClawbackOp;
    }
}