// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct ClaimLiquidityAtom
//  {
//      PoolID liquidityPoolID;
//  
//      // amount and asset taken from the pool
//      Asset assetSold;
//      int64 amountSold;
//  
//      // amount and asset sent to the pool
//      Asset assetBought;
//      int64 amountBought;
//  };

//  ===========================================================================
public class ClaimLiquidityAtom
{
    public PoolID LiquidityPoolID { get; set; }
    public Asset AssetSold { get; set; }
    public Int64 AmountSold { get; set; }
    public Asset AssetBought { get; set; }
    public Int64 AmountBought { get; set; }

    public static void Encode(XdrDataOutputStream stream, ClaimLiquidityAtom encodedClaimLiquidityAtom)
    {
        PoolID.Encode(stream, encodedClaimLiquidityAtom.LiquidityPoolID);
        Asset.Encode(stream, encodedClaimLiquidityAtom.AssetSold);
        Int64.Encode(stream, encodedClaimLiquidityAtom.AmountSold);
        Asset.Encode(stream, encodedClaimLiquidityAtom.AssetBought);
        Int64.Encode(stream, encodedClaimLiquidityAtom.AmountBought);
    }

    public static ClaimLiquidityAtom Decode(XdrDataInputStream stream)
    {
        var decodedClaimLiquidityAtom = new ClaimLiquidityAtom();
        decodedClaimLiquidityAtom.LiquidityPoolID = PoolID.Decode(stream);
        decodedClaimLiquidityAtom.AssetSold = Asset.Decode(stream);
        decodedClaimLiquidityAtom.AmountSold = Int64.Decode(stream);
        decodedClaimLiquidityAtom.AssetBought = Asset.Decode(stream);
        decodedClaimLiquidityAtom.AmountBought = Int64.Decode(stream);
        return decodedClaimLiquidityAtom;
    }
}