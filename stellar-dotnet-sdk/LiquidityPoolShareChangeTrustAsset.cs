using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LiquidityPoolShareChangeTrustAsset : ChangeTrustAsset
{
    public const string RestApiType = "pool_share";

    public LiquidityPoolShareChangeTrustAsset(LiquidityPoolParameters parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters), "parameters cannot be null");
    }

    /// <summary>
    /// Constructs a new <c>LiquidityPoolShareChangeTrustAsset</c> object.
    /// </summary>
    /// <param name="assetA"></param>
    /// <param name="assetB"></param>
    /// <param name="feeBP"></param>
    public LiquidityPoolShareChangeTrustAsset(Asset assetA, Asset assetB, int feeBP)
    {
        Parameters = new LiquidityPoolConstantProductParameters(assetA, assetB, feeBP);
    }
    
    public LiquidityPoolParameters Parameters { get; set; }

    public override string Type => RestApiType;

    public LiquidityPoolID GetLiquidityPoolID()
    {
        return Parameters.GetID();
    }

    public override string ToString()
    {
        return GetLiquidityPoolID().ToString();
    }

    public override int GetHashCode()
    {
        return Parameters.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is LiquidityPoolShareChangeTrustAsset other && Parameters.Equals(other.Parameters);
    }

    public override int CompareTo(ChangeTrustAsset asset)
    {
        return asset.Type != RestApiType
            ? 1
            : string.Compare(ToString(), ((LiquidityPoolShareChangeTrustAsset)asset).ToString(),
                StringComparison.Ordinal);
    }

    public override xdr.ChangeTrustAsset ToXdr()
    {
        var changeTrustXdr = new xdr.ChangeTrustAsset
        {
            Discriminant =
            {
                InnerValue = AssetType.AssetTypeEnum.ASSET_TYPE_POOL_SHARE
            }
        };

        var parameters = Parameters.ToXdr();
        changeTrustXdr.LiquidityPool = parameters;

        return changeTrustXdr;
    }
}