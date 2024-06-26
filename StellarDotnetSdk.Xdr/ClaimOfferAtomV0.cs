// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct ClaimOfferAtomV0
//  {
//      // emitted to identify the offer
//      uint256 sellerEd25519; // Account that owns the offer
//      int64 offerID;
//  
//      // amount and asset taken from the owner
//      Asset assetSold;
//      int64 amountSold;
//  
//      // amount and asset sent to the owner
//      Asset assetBought;
//      int64 amountBought;
//  };

//  ===========================================================================
public class ClaimOfferAtomV0
{
    public Uint256 SellerEd25519 { get; set; }
    public Int64 OfferID { get; set; }
    public Asset AssetSold { get; set; }
    public Int64 AmountSold { get; set; }
    public Asset AssetBought { get; set; }
    public Int64 AmountBought { get; set; }

    public static void Encode(XdrDataOutputStream stream, ClaimOfferAtomV0 encodedClaimOfferAtomV0)
    {
        Uint256.Encode(stream, encodedClaimOfferAtomV0.SellerEd25519);
        Int64.Encode(stream, encodedClaimOfferAtomV0.OfferID);
        Asset.Encode(stream, encodedClaimOfferAtomV0.AssetSold);
        Int64.Encode(stream, encodedClaimOfferAtomV0.AmountSold);
        Asset.Encode(stream, encodedClaimOfferAtomV0.AssetBought);
        Int64.Encode(stream, encodedClaimOfferAtomV0.AmountBought);
    }

    public static ClaimOfferAtomV0 Decode(XdrDataInputStream stream)
    {
        var decodedClaimOfferAtomV0 = new ClaimOfferAtomV0();
        decodedClaimOfferAtomV0.SellerEd25519 = Uint256.Decode(stream);
        decodedClaimOfferAtomV0.OfferID = Int64.Decode(stream);
        decodedClaimOfferAtomV0.AssetSold = Asset.Decode(stream);
        decodedClaimOfferAtomV0.AmountSold = Int64.Decode(stream);
        decodedClaimOfferAtomV0.AssetBought = Asset.Decode(stream);
        decodedClaimOfferAtomV0.AmountBought = Int64.Decode(stream);
        return decodedClaimOfferAtomV0;
    }
}