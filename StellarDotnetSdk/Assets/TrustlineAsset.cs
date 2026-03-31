using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;
using LiquidityPoolParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolParameters;

namespace StellarDotnetSdk.Assets;

/// <summary>
///     Represents an asset that can appear in a trustline on the Stellar network.
///     This includes standard assets (native, credit_alphanum4, credit_alphanum12) and liquidity pool shares.
/// </summary>
public abstract class TrustlineAsset
{
    /// <summary>
    ///     Gets the asset type string (e.g., <c>"native"</c>, <c>"credit_alphanum4"</c>, <c>"credit_alphanum12"</c>, or <c>"pool_share"</c>).
    /// </summary>
    public abstract string Type { get; }

    /// <summary>
    ///     Creates a <see cref="TrustlineAsset" /> from its canonical string form.
    /// </summary>
    /// <param name="canonicalForm">The canonical asset string (e.g., <c>"native"</c> or <c>"USD:GABC..."</c>).</param>
    /// <returns>A <see cref="TrustlineAsset" /> wrapping the parsed asset.</returns>
    public static TrustlineAsset Create(string canonicalForm)
    {
        return new Wrapper(Asset.Create(canonicalForm));
    }

    /// <summary>
    ///     Creates a <see cref="TrustlineAsset" /> from an asset type, code, and issuer.
    /// </summary>
    /// <param name="type">The asset type (e.g., <c>"native"</c>).</param>
    /// <param name="code">The asset code.</param>
    /// <param name="issuer">The issuer account ID.</param>
    /// <returns>A <see cref="TrustlineAsset" /> wrapping the created asset.</returns>
    public static TrustlineAsset Create(string type, string code, string issuer)
    {
        return new Wrapper(Asset.Create(type, code, issuer));
    }

    /// <summary>
    ///     Creates a <see cref="TrustlineAsset" /> by wrapping an existing <see cref="Asset" />.
    /// </summary>
    /// <param name="asset">The asset to wrap.</param>
    /// <returns>A <see cref="TrustlineAsset" /> wrapping the given asset.</returns>
    public static TrustlineAsset Create(Asset asset)
    {
        return new Wrapper(asset);
    }

    /// <summary>
    ///     Creates a <see cref="LiquidityPoolShareTrustlineAsset" /> from liquidity pool parameters.
    /// </summary>
    /// <param name="parameters">The liquidity pool parameters.</param>
    /// <returns>A <see cref="LiquidityPoolShareTrustlineAsset" /> for the given pool.</returns>
    public static TrustlineAsset Create(LiquidityPoolParameters parameters)
    {
        return new LiquidityPoolShareTrustlineAsset(parameters);
    }

    /// <summary>
    ///     Creates a <see cref="LiquidityPoolShareTrustlineAsset" /> from a pool ID.
    /// </summary>
    /// <param name="id">The liquidity pool ID.</param>
    /// <returns>A <see cref="LiquidityPoolShareTrustlineAsset" /> referencing the given pool.</returns>
    public static TrustlineAsset Create(LiquidityPoolId id)
    {
        return new LiquidityPoolShareTrustlineAsset(id);
    }

    /// <summary>
    ///     Creates a <see cref="TrustlineAsset" /> from a <see cref="ChangeTrustAsset.Wrapper" />.
    /// </summary>
    /// <param name="wrapper">The change trust asset wrapper containing the underlying asset.</param>
    /// <returns>A <see cref="TrustlineAsset" /> wrapping the underlying asset.</returns>
    public static TrustlineAsset Create(ChangeTrustAsset.Wrapper wrapper)
    {
        return new Wrapper(wrapper.Asset);
    }

    /// <summary>
    ///     Creates a <see cref="LiquidityPoolShareTrustlineAsset" /> from a <see cref="LiquidityPoolShareChangeTrustAsset" />.
    /// </summary>
    /// <param name="share">The liquidity pool share change trust asset.</param>
    /// <returns>A <see cref="LiquidityPoolShareTrustlineAsset" /> using the pool parameters from the change trust asset.</returns>
    public static TrustlineAsset Create(LiquidityPoolShareChangeTrustAsset share)
    {
        return new LiquidityPoolShareTrustlineAsset(share.Parameters);
    }

    /// <summary>
    ///     Creates a <see cref="TrustlineAsset" /> for a non-native credit asset.
    /// </summary>
    /// <param name="code">The asset code.</param>
    /// <param name="issuer">The issuer account ID.</param>
    /// <returns>A <see cref="TrustlineAsset" /> wrapping the credit asset.</returns>
    public static TrustlineAsset CreateNonNativeAsset(string code, string issuer)
    {
        return Create(Asset.CreateNonNativeAsset(code, issuer));
    }

    /// <summary>
    ///     Creates a <see cref="TrustlineAsset" /> from an XDR <see cref="TrustLineAsset" /> object.
    /// </summary>
    /// <param name="trustLineAssetXdr">The XDR trustline asset to deserialize.</param>
    /// <returns>The deserialized <see cref="TrustlineAsset" />.</returns>
    /// <exception cref="ArgumentException">Thrown when the asset type is unknown.</exception>
    public static TrustlineAsset FromXdr(TrustLineAsset trustLineAssetXdr)
    {
        return trustLineAssetXdr.Discriminant.InnerValue switch
        {
            AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE => Create(new AssetTypeNative()),
            AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4
                => Create(new AssetTypeCreditAlphaNum4(
                    Util.PaddedByteArrayToString(trustLineAssetXdr.AlphaNum4.AssetCode.InnerValue),
                    KeyPair.FromXdrPublicKey(trustLineAssetXdr.AlphaNum4.Issuer.InnerValue).AccountId)),
            AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM12
                => Create(new AssetTypeCreditAlphaNum12(
                    Util.PaddedByteArrayToString(trustLineAssetXdr.AlphaNum12.AssetCode.InnerValue),
                    KeyPair.FromXdrPublicKey(trustLineAssetXdr.AlphaNum12.Issuer.InnerValue).AccountId)),
            AssetType.AssetTypeEnum.ASSET_TYPE_POOL_SHARE
                => new LiquidityPoolShareTrustlineAsset(LiquidityPoolId.FromXdr(trustLineAssetXdr.LiquidityPoolID)),
            _ => throw new ArgumentException($"Unknown asset type {trustLineAssetXdr.Discriminant.InnerValue}"),
        };
    }

    /// <inheritdoc />
    public new abstract bool Equals(object obj);

    /// <summary>
    ///     Compares this trustline asset to another for ordering.
    /// </summary>
    /// <param name="asset">The trustline asset to compare with.</param>
    /// <returns>A negative value, zero, or positive value for ordering.</returns>
    public abstract int CompareTo(TrustlineAsset asset);

    /// <summary>
    ///     Converts this trustline asset to its XDR <see cref="TrustLineAsset" /> representation.
    /// </summary>
    /// <returns>A <see cref="TrustLineAsset" /> XDR object corresponding to the concrete asset type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the trustline asset type is not recognized.</exception>
    public TrustLineAsset ToXdr()
    {
        return this switch
        {
            Wrapper wrapper => wrapper.ToXdrTrustLineAsset(),
            LiquidityPoolShareTrustlineAsset poolShareTrustlineAsset => poolShareTrustlineAsset.ToXdrTrustLineAsset(),
            _ => throw new InvalidOperationException("Unknown TrustLineAsset type"),
        };
    }

    /// <summary>
    ///     Wraps a standard <see cref="Asset" /> instance as a <see cref="TrustlineAsset" />,
    ///     enabling non-pool-share assets to be used where a trustline asset is required.
    /// </summary>
    public class Wrapper : TrustlineAsset
    {
        /// <summary>
        ///     Initializes a new <see cref="Wrapper" /> around the specified asset.
        /// </summary>
        /// <param name="asset">The asset to wrap. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="asset" /> is null.</exception>
        public Wrapper(Asset asset)
        {
            Asset = asset ?? throw new ArgumentNullException(nameof(asset), "asset cannot be null");
        }

        /// <summary>
        ///     Gets or sets the underlying <see cref="Assets.Asset" />.
        /// </summary>
        public Asset Asset { get; set; }

        /// <inheritdoc />
        public override string Type => Asset.Type;

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is not Wrapper other)
            {
                return false;
            }
            return Asset.Equals(other.Asset);
        }

        /// <inheritdoc />
        public override int CompareTo(TrustlineAsset asset)
        {
            if (asset.Type == LiquidityPoolShareTrustlineAsset.RestApiType)
            {
                return -1;
            }
            return Asset.CompareTo(((Wrapper)asset).Asset);
        }

        /// <summary>
        ///     Converts the wrapped asset to its XDR <see cref="TrustLineAsset" /> representation.
        /// </summary>
        /// <returns>A <see cref="TrustLineAsset" /> XDR object with the discriminant and asset data from the wrapped asset.</returns>
        public TrustLineAsset ToXdrTrustLineAsset()
        {
            var trustlineAssetXdr = new TrustLineAsset();

            var assetXdr = Asset.ToXdr();
            trustlineAssetXdr.Discriminant = assetXdr.Discriminant;
            trustlineAssetXdr.AlphaNum4 = assetXdr.AlphaNum4;
            trustlineAssetXdr.AlphaNum12 = assetXdr.AlphaNum12;
            return trustlineAssetXdr;
        }
    }
}