using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the claimable_balance_claimed effect response.
///     This effect occurs when a claimable balance is claimed.
/// </summary>
public sealed class ClaimableBalanceClaimedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 52;

    /// <summary>
    ///     The asset that was claimed (in canonical form: "code:issuer" or "native").
    /// </summary>
    [JsonPropertyName("asset")]
    public string? Asset { get; init; }

    /// <summary>
    ///     The unique identifier of the claimable balance that was claimed.
    /// </summary>
    [JsonPropertyName("balance_id")]
    public string? BalanceId { get; init; }

    /// <summary>
    ///     The amount that was claimed.
    /// </summary>
    [JsonPropertyName("amount")]
    public string? Amount { get; init; }
}