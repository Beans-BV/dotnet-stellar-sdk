using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using LiquidityPoolParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolParameters;

namespace StellarDotnetSdk.Assets;

public abstract class ChangeTrustAsset
{
    public abstract string Type { get; }

    public static ChangeTrustAsset Create(string canonicalForm)
    {
        return new Wrapper(Asset.Create(canonicalForm));
    }

    public static ChangeTrustAsset Create(string type, string code, string issuer)
    {
        return new Wrapper(Asset.Create(type, code, issuer));
    }

    public static ChangeTrustAsset Create(Asset asset)
    {
        return new Wrapper(asset);
    }

    public static ChangeTrustAsset Create(LiquidityPoolParameters parameters)
    {
        return new LiquidityPoolShareChangeTrustAsset(parameters);
    }

    public static ChangeTrustAsset Create(Asset assetA, Asset assetB, int feeBp)
    {
        return new LiquidityPoolShareChangeTrustAsset(assetA, assetB, feeBp);
    }

    public static ChangeTrustAsset Create(TrustlineAsset.Wrapper wrapper)
    {
        return new Wrapper(wrapper.Asset);
    }

    public static ChangeTrustAsset CreateNonNativeAsset(string code, string issuer)
    {
        return Create(Asset.CreateNonNativeAsset(code, issuer));
    }

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

    public new abstract bool Equals(object obj);

    public abstract int CompareTo(ChangeTrustAsset asset);

    public abstract Xdr.ChangeTrustAsset ToXdr();

    public class Wrapper : ChangeTrustAsset
    {
        public Wrapper(Asset asset)
        {
            Asset = asset ?? throw new ArgumentNullException(nameof(asset), "asset cannot be null");
        }

        public Asset Asset { get; set; }

        public override string Type => Asset.Type;

        public override bool Equals(object? obj)
        {
            return obj is Wrapper other && Asset.Equals(other.Asset);
        }

        public override int GetHashCode()
        {
            return Asset.GetHashCode();
        }

        public override int CompareTo(ChangeTrustAsset asset)
        {
            if (asset.Type == "pool_share")
            {
                return -1;
            }

            return Asset.CompareTo(((Wrapper)asset).Asset);
        }

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