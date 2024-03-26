using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LiquidityPoolShareTrustlineAsset : TrustlineAsset
{
    public const string RestApiType = "pool_share";

    public LiquidityPoolShareTrustlineAsset(LiquidityPoolParameters parameters)
    {
        ID = parameters.GetID() ?? throw new ArgumentNullException(nameof(parameters), "parameters cannot be null");
    }

    public LiquidityPoolShareTrustlineAsset(LiquidityPoolID id)
    {
        ID = id ?? throw new ArgumentNullException(nameof(id), "id cannot be null");
    }

    public LiquidityPoolID ID { get; }

    public override string Type => RestApiType;

    public override string ToString()
    {
        return ID.ToString();
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not LiquidityPoolShareTrustlineAsset other) return false;

        return obj.ToString() == other.ToString();
    }

    public override int CompareTo(TrustlineAsset asset)
    {
        return asset.Type != RestApiType
            ? 1
            : string.Compare(ToString(), ((LiquidityPoolShareTrustlineAsset)asset).ToString(),
                StringComparison.Ordinal);
    }

    public TrustLineAsset ToXdrTrustLineAsset()
    {
        var xdr = new TrustLineAsset
        {
            Discriminant = AssetType.Create(AssetType.AssetTypeEnum.ASSET_TYPE_POOL_SHARE),
            LiquidityPoolID = ID.ToXdr()
        };
        return xdr;
    }
}