// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct ClaimClaimableBalanceOp
//  {
//      ClaimableBalanceID balanceID;
//  };

//  ===========================================================================
public class ClaimClaimableBalanceOp
{
    public ClaimableBalanceID BalanceID { get; set; }

    public static void Encode(XdrDataOutputStream stream, ClaimClaimableBalanceOp encodedClaimClaimableBalanceOp)
    {
        ClaimableBalanceID.Encode(stream, encodedClaimClaimableBalanceOp.BalanceID);
    }

    public static ClaimClaimableBalanceOp Decode(XdrDataInputStream stream)
    {
        var decodedClaimClaimableBalanceOp = new ClaimClaimableBalanceOp();
        decodedClaimClaimableBalanceOp.BalanceID = ClaimableBalanceID.Decode(stream);
        return decodedClaimClaimableBalanceOp;
    }
}