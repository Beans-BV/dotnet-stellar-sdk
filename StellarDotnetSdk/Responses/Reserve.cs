using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a reserve in a liquidity pool, containing an asset and its amount.
/// </summary>
[JsonConverter(typeof(ReserveJsonConverter))]
public sealed class Reserve
{
    /// <summary>
    ///     The amount of this asset held in reserve.
    ///     Represented as a string to preserve precision (up to 7 decimal places).
    /// </summary>
    [JsonPropertyName("amount")]
    public required string Amount { get; init; }

    /// <summary>
    ///     The asset held in this reserve.
    /// </summary>
    [JsonPropertyName("asset")]
    public required Asset Asset { get; init; }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not Reserve other)
        {
            return false;
        }

        return Equals(Asset, other.Asset) && Equals(Amount, other.Amount);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = 1588693772;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Amount);
        hashCode = hashCode * -1521134295 + EqualityComparer<Asset>.Default.GetHashCode(Asset);
        return hashCode;
    }
}