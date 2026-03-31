using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using LiquidityPoolParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolParameters;

namespace StellarDotnetSdk.Assets;

/// <summary>
///     Represents an asset that can be used in a ChangeTrust operation.
///     This includes standard Stellar assets (native, credit_alphanum4, credit_alphanum12) and liquidity pool shares.
/// </summary>
public abstract class ChangeTrustAsset
{
    /// <summary>
    ///     Gets the asset type string (e.g., <c>"native"</c>, <c>"credit_alphanum4"</c>, <c>"credit_alphanum12"</c>, or <c>"pool_share"</c>).
    /// </summary>
    public abstract string Type { get; }

    /// <summary>
    ///     Creates a <see cref="ChangeTrustAsset" /> from its canonical string form.
    /// </summary>
    /// <param name="canonicalForm">The canonical asset string (e.g., <c>"native"</c> or <c>"USD:GABC..."</c>).</param>
    /// <returns>A <see cref="ChangeTrustAsset" /> wrapping the parsed asset.</returns>
    public static ChangeTrustAsset Create(string canonicalForm)
    {
        return new Wrapper(Asset.Create(canonicalForm));
    }

    /// <summary>
    ///     Creates a <see cref="ChangeTrustAsset" /> from an asset type, code, and issuer.
    /// </summary>
    /// <param name="type">The asset type (e.g., <c>"native"</c>).</param>
    /// <param name="code">The asset code.</param>
    /// <param name="issuer">The issuer account ID.</param>
    /// <returns>A <see cref="ChangeTrustAsset" /> wrapping the created asset.</returns>
    public static ChangeTrustAsset Create(string type, string code, string issuer)
    {
        return new Wrapper(Asset.Create(type, code, issuer));
    }

    /// <summary>
    ///     Creates a <see cref="ChangeTrustAsset" /> by wrapping an existing <see cref="Asset" />.
    /// </summary>
    /// <param name="asset">The asset to wrap.</param>
    /// <returns>A <see cref="ChangeTrustAsset" /> wrapping the given asset.</returns>
    public static ChangeTrustAsset Create(Asset asset)
    {
        return new Wrapper(asset);
    }

    /// <summary>
    ///     Creates a <see cref="LiquidityPoolShareChangeTrustAsset" /> from liquidity pool parameters.
    /// </summary>
    /// <param name="parameters">The liquidity pool parameters.</param>
    /// <returns>A <see cref="LiquidityPoolShareChangeTrustAsset" /> for the given pool.</returns>
    public static ChangeTrustAsset Create(LiquidityPoolParameters parameters)
    {
        return new LiquidityPoolShareChangeTrustAsset(parameters);
    }

    /// <summary>
    ///     Creates a <see cref="LiquidityPoolShareChangeTrustAsset" /> from a pair of assets and a fee.
    /// </summary>
    /// <param name="assetA">The first asset in the pool.</param>
    /// <param name="assetB">The second asset in the pool.</param>
    /// <param name="feeBp">The pool fee in basis points.</param>
    /// <returns>A <see cref="LiquidityPoolShareChangeTrustAsset" /> for the given pool parameters.</returns>
    public static ChangeTrustAsset Create(Asset assetA, Asset assetB, int feeBp)
    {
        return new LiquidityPoolShareChangeTrustAsset(assetA, assetB, feeBp);
    }

    /// <summary>
    ///     Creates a <see cref="ChangeTrustAsset" /> from a <see cref="TrustlineAsset.Wrapper" />.
    /// </summary>
    /// <param name="wrapper">The trustline asset wrapper containing the underlying asset.</param>
    /// <returns>A <see cref="ChangeTrustAsset" /> wrapping the underlying asset.</returns>
    public static ChangeTrustAsset Create(TrustlineAsset.Wrapper wrapper)
    {
        return new Wrapper(wrapper.Asset);
    }

    /// <summary>
    ///     Creates a <see cref="ChangeTrustAsset" /> for a non-native credit asset.
    /// </summary>
    /// <param name="code">The asset code.</param>
    /// <param name="issuer">The issuer account ID.</param>
    /// <returns>A <see cref="ChangeTrustAsset" /> wrapping the credit asset.</returns>
    public static ChangeTrustAsset CreateNonNativeAsset(string code, string issuer)
    {
        return Create(Asset.CreateNonNativeAsset(code, issuer));
    }

    /// <summary>
    ///     Creates a <see cref="ChangeTrustAsset" /> from an XDR <c>ChangeTrustAsset</c> object.
    /// </summary>
    /// <param name="changeTrustXdr">The XDR change trust asset to deserialize.</param>
    /// <returns>The deserialized <see cref="ChangeTrustAsset" />.</returns>
    /// <exception cref="ArgumentException">Thrown when the asset type is unknown.</exception>
    public static ChangeTrustAsset FromXdr(Xdr.ChangeTrustAsset changeTrustXdr)
    {
        string accountId;
        string assetCode;

        switch (changeTrustXdr.Discriminant.InnerValue)
        {
            case AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE:
                return Create(new AssetTypeNative());

            case AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4:
                assetCode = Util.PaddedByteArrayToString(changeTrustXdr.AlphaNum4.AssetCode.InnerValue);
                accountId = KeyPair.FromXdrPublicKey(changeTrustXdr.AlphaNum4.Issuer.InnerValue).AccountId;
                return Create(new AssetTypeCreditAlphaNum4(assetCode, accountId));

            case AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM12:
                assetCode = Util.PaddedByteArrayToString(changeTrustXdr.AlphaNum12.AssetCode.InnerValue);
                accountId = KeyPair.FromXdrPublicKey(changeTrustXdr.AlphaNum12.Issuer.InnerValue).AccountId;
                return Create(new AssetTypeCreditAlphaNum12(assetCode, accountId));

            case AssetType.AssetTypeEnum.ASSET_TYPE_POOL_SHARE:
                return new LiquidityPoolShareChangeTrustAsset(
                    LiquidityPoolParameters.FromXdr(changeTrustXdr.LiquidityPool));

            default:
                throw new ArgumentException($"Unknown asset type {changeTrustXdr.Discriminant.InnerValue}");
        }
    }

    /// <inheritdoc />
    public new abstract bool Equals(object obj);

    /// <summary>
    ///     Compares this change trust asset to another for ordering.
    /// </summary>
    /// <param name="asset">The change trust asset to compare with.</param>
    /// <returns>A negative value, zero, or positive value for ordering.</returns>
    public abstract int CompareTo(ChangeTrustAsset asset);

    /// <summary>
    ///     Serializes this change trust asset to its XDR representation.
    /// </summary>
    /// <returns>An XDR <c>ChangeTrustAsset</c> object.</returns>
    public abstract Xdr.ChangeTrustAsset ToXdr();

    /// <summary>
    ///     Wraps a standard <see cref="Asset" /> instance as a <see cref="ChangeTrustAsset" />,
    ///     enabling non-pool-share assets to be used in change trust operations.
    /// </summary>
    public class Wrapper : ChangeTrustAsset
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
            return obj is Wrapper other && Asset.Equals(other.Asset);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Asset.GetHashCode();
        }

        /// <inheritdoc />
        public override int CompareTo(ChangeTrustAsset asset)
        {
            if (asset.Type == "pool_share")
            {
                return -1;
            }

            return Asset.CompareTo(((Wrapper)asset).Asset);
        }

        /// <inheritdoc />
        public override Xdr.ChangeTrustAsset ToXdr()
        {
            var changeTrustXdr = new Xdr.ChangeTrustAsset();

            var assetXdr = Asset.ToXdr();
            changeTrustXdr.Discriminant = assetXdr.Discriminant;
            changeTrustXdr.AlphaNum4 = assetXdr.AlphaNum4;
            changeTrustXdr.AlphaNum12 = assetXdr.AlphaNum12;

            return changeTrustXdr;
        }
    }
}