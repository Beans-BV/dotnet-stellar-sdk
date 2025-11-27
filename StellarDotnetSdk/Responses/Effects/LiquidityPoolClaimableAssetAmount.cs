using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents an asset amount with an optional claimable balance ID.
///     Used in liquidity pool revocation effects.
/// </summary>
[JsonConverter(typeof(LiquidityPoolClaimableAssetAmountJsonConverter))]
public sealed class LiquidityPoolClaimableAssetAmount
{
    /// <summary>
    ///     The asset.
    /// </summary>
    [JsonPropertyName("asset")]
    public Asset? Asset { get; init; }

    /// <summary>
    ///     The amount of the asset.
    /// </summary>
    [JsonPropertyName("amount")]
    public string? Amount { get; init; }

    /// <summary>
    ///     The claimable balance ID if the revoked amount was placed in a claimable balance.
    /// </summary>
    [JsonPropertyName("claimable_balance_id")]
    public string? ClaimableBalanceId { get; init; }
}