using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class AssetTypeNative : Asset
{
    public const string RestApiType = "native";

    public override string Type => RestApiType;

    public override int GetHashCode()
    {
        return 0;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not AssetTypeNative) return false;
        return GetHashCode() == obj.GetHashCode();
    }

    public override xdr.Asset ToXdr()
    {
        var thisXdr = new xdr.Asset
        {
            Discriminant = AssetType.Create(AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE)
        };
        return thisXdr;
    }

    /// <inheritdoc />
    public override string CanonicalName()
    {
        return "native";
    }

    public override int CompareTo(Asset asset)
    {
        if (asset.Type == RestApiType) return 0;
        return -1;
    }
}