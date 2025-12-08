using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the claimable_balance_created effect response.
///     This effect occurs when a new claimable balance is created.
/// </summary>
public sealed class ClaimableBalanceCreatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 50;

    /// <summary>
    ///     The asset in the claimable balance (in canonical form: "code:issuer" or "native").
    /// </summary>
    [JsonPropertyName("asset")]
    public required string Asset { get; init; }

    /// <summary>
    ///     The unique identifier of the claimable balance.
    /// </summary>
    [JsonPropertyName("balance_id")]
    public required string BalanceId { get; init; }

    /// <summary>
    ///     The amount of the asset in the claimable balance.
    /// </summary>
    [JsonPropertyName("amount")]
    public required string Amount { get; init; }
}