using System;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;
using LiquidityPoolParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolParameters;

namespace StellarDotnetSdk.Assets;

/// <summary>
///     Represents a liquidity pool share as a trustline asset.
///     This is used to reference an existing liquidity pool trustline by its pool ID.
/// </summary>
public class LiquidityPoolShareTrustlineAsset : TrustlineAsset
{
    public const string RestApiType = "pool_share";

    public LiquidityPoolShareTrustlineAsset(LiquidityPoolParameters parameters)
    {
        Id = parameters.GetId() ?? throw new ArgumentNullException(nameof(parameters), "parameters cannot be null");
    }

    public LiquidityPoolShareTrustlineAsset(LiquidityPoolId id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id), "id cannot be null");
    }

    public LiquidityPoolId Id { get; }

    public override string Type => RestApiType;

    public override string ToString()
    {
        return Id.ToString();
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not LiquidityPoolShareTrustlineAsset other)
        {
            return false;
        }

        return obj.ToString() == other.ToString();
    }

    public override int CompareTo(TrustlineAsset asset)
    {
        return asset.Type != RestApiType
            ? 1
            : string.Compare(ToString(), ((LiquidityPoolShareTrustlineAsset)asset).ToString(),
                StringComparison.Ordinal);
    }

    /// <summary>
    ///     Converts this liquidity pool share trustline asset to its XDR <see cref="TrustLineAsset" /> representation.
    /// </summary>
    /// <returns>A <see cref="TrustLineAsset" /> XDR object with the pool share discriminant and pool ID.</returns>
    public TrustLineAsset ToXdrTrustLineAsset()
    {
        var xdr = new TrustLineAsset
        {
            Discriminant = AssetType.Create(AssetType.AssetTypeEnum.ASSET_TYPE_POOL_SHARE),
            LiquidityPoolID = Id.ToXdr(),
        };
        return xdr;
    }
}