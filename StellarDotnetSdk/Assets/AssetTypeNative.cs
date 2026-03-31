using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Assets;

/// <summary>
///     Represents the native Stellar asset (XLM/Lumens). This is the built-in currency of the Stellar network.
/// </summary>
public class AssetTypeNative : Asset
{
    /// <summary>
    ///     The Horizon REST API type identifier for the native asset.
    /// </summary>
    public const string RestApiType = "native";

    /// <inheritdoc />
    public override string Type => RestApiType;

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return 0;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not AssetTypeNative)
        {
            return false;
        }
        return GetHashCode() == obj.GetHashCode();
    }

    /// <inheritdoc />
    public override Xdr.Asset ToXdr()
    {
        var thisXdr = new Xdr.Asset
        {
            Discriminant = AssetType.Create(AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE),
        };
        return thisXdr;
    }

    /// <inheritdoc />
    public override string CanonicalName()
    {
        return "native";
    }

    /// <inheritdoc />
    public override int CompareTo(Asset asset)
    {
        if (asset.Type == RestApiType)
        {
            return 0;
        }
        return -1;
    }
}