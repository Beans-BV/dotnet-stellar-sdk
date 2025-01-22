using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Assets;

/// <summary>
///     Class to have Asset and Amount in the same place.
/// </summary>
[JsonConverter(typeof(AssetAmountJsonConverter))]
public class AssetAmount
{
    public AssetAmount(Asset asset, string amount)
    {
        Asset = asset;
        Amount = amount;
    }

    public Asset Asset { get; }

    public string Amount { get; }

    public override int GetHashCode()
    {
        return Asset.GetHashCode().Hash(Amount);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not AssetAmount other)
        {
            return false;
        }

        return Equals(Asset, other.Asset) && Equals(Amount, other.Amount);
    }
}