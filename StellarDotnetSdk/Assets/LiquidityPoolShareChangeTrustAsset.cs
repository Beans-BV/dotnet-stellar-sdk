using System;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;
using LiquidityPoolConstantProductParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolConstantProductParameters;
using LiquidityPoolParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolParameters;

namespace StellarDotnetSdk.Assets;

/// <summary>
///     Represents a liquidity pool share asset used in change trust operations.
///     This asset type allows accounts to establish trustlines to liquidity pools on the Stellar network.
/// </summary>
public class LiquidityPoolShareChangeTrustAsset : ChangeTrustAsset
{
    public const string RestApiType = "pool_share";

    public LiquidityPoolShareChangeTrustAsset(LiquidityPoolParameters parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters), "parameters cannot be null");
    }

    /// <summary>
    ///     Constructs a new <c>LiquidityPoolShareChangeTrustAsset</c> object.
    /// </summary>
    /// <param name="assetA"></param>
    /// <param name="assetB"></param>
    /// <param name="feeBp"></param>
    public LiquidityPoolShareChangeTrustAsset(Asset assetA, Asset assetB, int feeBp)
    {
        Parameters = new LiquidityPoolConstantProductParameters(assetA, assetB, feeBp);
    }

    public LiquidityPoolParameters Parameters { get; set; }

    public override string Type => RestApiType;

    /// <summary>
    ///     Gets the unique identifier of the liquidity pool associated with this asset.
    /// </summary>
    /// <returns>The <see cref="LiquidityPoolId" /> derived from the pool parameters.</returns>
    public LiquidityPoolId GetLiquidityPoolId()
    {
        return Parameters.GetId();
    }

    public override string ToString()
    {
        return GetLiquidityPoolId().ToString();
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

    public override Xdr.ChangeTrustAsset ToXdr()
    {
        var xdr = new Xdr.ChangeTrustAsset
        {
            Discriminant = AssetType.Create(AssetType.AssetTypeEnum.ASSET_TYPE_POOL_SHARE),
            LiquidityPool = Parameters.ToXdr(),
        };
        return xdr;
    }
}