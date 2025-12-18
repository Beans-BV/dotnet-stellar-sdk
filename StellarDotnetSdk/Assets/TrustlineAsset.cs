using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;
using LiquidityPoolParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolParameters;

namespace StellarDotnetSdk.Assets;

public abstract class TrustlineAsset
{
    public abstract string Type { get; }

    public static TrustlineAsset Create(string canonicalForm)
    {
        return new Wrapper(Asset.Create(canonicalForm));
    }

    public static TrustlineAsset Create(string type, string code, string issuer)
    {
        return new Wrapper(Asset.Create(type, code, issuer));
    }

    public static TrustlineAsset Create(Asset asset)
    {
        return new Wrapper(asset);
    }

    public static TrustlineAsset Create(LiquidityPoolParameters parameters)
    {
        return new LiquidityPoolShareTrustlineAsset(parameters);
    }

    public static TrustlineAsset Create(LiquidityPoolId id)
    {
        return new LiquidityPoolShareTrustlineAsset(id);
    }

    public static TrustlineAsset Create(ChangeTrustAsset.Wrapper wrapper)
    {
        return new Wrapper(wrapper.Asset);
    }

    public static TrustlineAsset Create(LiquidityPoolShareChangeTrustAsset share)
    {
        return new LiquidityPoolShareTrustlineAsset(share.Parameters);
    }

    public static TrustlineAsset CreateNonNativeAsset(string code, string issuer)
    {
        return Create(Asset.CreateNonNativeAsset(code, issuer));
    }

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

    public new abstract bool Equals(object obj);

    public abstract int CompareTo(TrustlineAsset asset);

    public TrustLineAsset ToXdr()
    {
        return this switch
        {
            Wrapper wrapper => wrapper.ToXdrTrustLineAsset(),
            LiquidityPoolShareTrustlineAsset poolShareTrustlineAsset => poolShareTrustlineAsset.ToXdrTrustLineAsset(),
            _ => throw new InvalidOperationException("Unknown TrustLineAsset type"),
        };
    }

    public class Wrapper : TrustlineAsset
    {
        public Wrapper(Asset asset)
        {
            Asset = asset ?? throw new ArgumentNullException(nameof(asset), "asset cannot be null");
        }

        public Asset Asset { get; set; }

        public override string Type => Asset.Type;

        public override bool Equals(object? obj)
        {
            if (obj is not Wrapper other)
            {
                return false;
            }
            return Asset.Equals(other.Asset);
        }

        public override int CompareTo(TrustlineAsset asset)
        {
            if (asset.Type == LiquidityPoolShareTrustlineAsset.RestApiType)
            {
                return -1;
            }
            return Asset.CompareTo(((Wrapper)asset).Asset);
        }

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