namespace StellarDotnetSdk.Assets;

/// <summary>
///     Class to have Asset and Amount in the same place.
/// </summary>
public class AssetAmount(Asset asset, string amount)
{
    public Asset Asset { get; } = asset;

    public string Amount { get; } = amount;

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