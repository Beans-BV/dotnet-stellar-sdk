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
    /// <summary>
    ///     The Horizon REST API type identifier for liquidity pool share assets.
    /// </summary>
    public const string RestApiType = "pool_share";

    /// <summary>
    ///     Initializes a new instance from existing liquidity pool parameters.
    /// </summary>
    /// <param name="parameters">The liquidity pool parameters. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameters" /> is null.</exception>
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

    /// <summary>
    ///     Gets or sets the liquidity pool parameters that define this pool share asset.
    /// </summary>
    public LiquidityPoolParameters Parameters { get; set; }

    /// <inheritdoc />
    public override string Type => RestApiType;

    /// <summary>
    ///     Gets the unique identifier of the liquidity pool associated with this asset.
    /// </summary>
    /// <returns>The <see cref="LiquidityPoolId" /> derived from the pool parameters.</returns>
    public LiquidityPoolId GetLiquidityPoolId()
    {
        return Parameters.GetId();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return GetLiquidityPoolId().ToString();
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Parameters.GetHashCode();
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return obj is LiquidityPoolShareChangeTrustAsset other && Parameters.Equals(other.Parameters);
    }

    /// <inheritdoc />
    public override int CompareTo(ChangeTrustAsset asset)
    {
        return asset.Type != RestApiType
            ? 1
            : string.Compare(ToString(), ((LiquidityPoolShareChangeTrustAsset)asset).ToString(),
                StringComparison.Ordinal);
    }

    /// <inheritdoc />
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