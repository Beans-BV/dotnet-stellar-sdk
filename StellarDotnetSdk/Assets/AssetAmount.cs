namespace StellarDotnetSdk.Assets;

/// <summary>
///     Class to have Asset and Amount in the same place.
/// </summary>
public class AssetAmount(
    Asset Asset,
    string Amount)
{
    /// <summary>
    ///     Gets the Stellar asset.
    /// </summary>
    public Asset Asset { get; } = Asset;

    /// <summary>
    ///     Gets the amount of the asset as a string (in stroops-compatible decimal format).
    /// </summary>
    public string Amount { get; } = Amount;

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Asset.GetHashCode().Hash(Amount);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not AssetAmount other)
        {
            return false;
        }

        return Equals(Asset, other.Asset) && Equals(Amount, other.Amount);
    }
}