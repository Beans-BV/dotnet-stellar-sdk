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
    /// <summary>
    ///     The Horizon REST API type identifier for liquidity pool share trustline assets.
    /// </summary>
    public const string RestApiType = "pool_share";

    /// <summary>
    ///     Initializes a new instance from liquidity pool parameters, deriving the pool ID.
    /// </summary>
    /// <param name="parameters">The liquidity pool parameters.</param>
    /// <exception cref="ArgumentNullException">Thrown when the derived pool ID is null.</exception>
    public LiquidityPoolShareTrustlineAsset(LiquidityPoolParameters parameters)
    {
        Id = parameters.GetId() ?? throw new ArgumentNullException(nameof(parameters), "parameters cannot be null");
    }

    /// <summary>
    ///     Initializes a new instance from an existing liquidity pool ID.
    /// </summary>
    /// <param name="id">The liquidity pool ID. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id" /> is null.</exception>
    public LiquidityPoolShareTrustlineAsset(LiquidityPoolId id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id), "id cannot be null");
    }

    /// <summary>
    ///     Gets the unique identifier of the liquidity pool this trustline references.
    /// </summary>
    public LiquidityPoolId Id { get; }

    /// <inheritdoc />
    public override string Type => RestApiType;

    /// <inheritdoc />
    public override string ToString()
    {
        return Id.ToString();
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not LiquidityPoolShareTrustlineAsset other)
        {
            return false;
        }

        return obj.ToString() == other.ToString();
    }

    /// <inheritdoc />
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