// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct LiquidityPoolConstantProductParameters
//  {
//      Asset assetA; // assetA < assetB
//      Asset assetB;
//      int32 fee; // Fee is in basis points, so the actual rate is (fee/100)%
//  };

//  ===========================================================================
public class LiquidityPoolConstantProductParameters
{
    public Asset AssetA { get; set; }
    public Asset AssetB { get; set; }
    public Int32 Fee { get; set; }

    public static void Encode(XdrDataOutputStream stream,
        LiquidityPoolConstantProductParameters encodedLiquidityPoolConstantProductParameters)
    {
        Asset.Encode(stream, encodedLiquidityPoolConstantProductParameters.AssetA);
        Asset.Encode(stream, encodedLiquidityPoolConstantProductParameters.AssetB);
        Int32.Encode(stream, encodedLiquidityPoolConstantProductParameters.Fee);
    }

    public static LiquidityPoolConstantProductParameters Decode(XdrDataInputStream stream)
    {
        var decodedLiquidityPoolConstantProductParameters = new LiquidityPoolConstantProductParameters();
        decodedLiquidityPoolConstantProductParameters.AssetA = Asset.Decode(stream);
        decodedLiquidityPoolConstantProductParameters.AssetB = Asset.Decode(stream);
        decodedLiquidityPoolConstantProductParameters.Fee = Int32.Decode(stream);
        return decodedLiquidityPoolConstantProductParameters;
    }
}