using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the claimable_balance_sponsorship_updated effect response.
///     This effect occurs when a claimable balance's sponsor changes.
/// </summary>
public sealed class ClaimableBalanceSponsorshipUpdatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 70;

    /// <summary>
    ///     The unique identifier of the claimable balance.
    /// </summary>
    [JsonPropertyName("balance_id")]
    public string? BalanceId { get; init; }

    /// <summary>
    ///     The account ID of the former sponsor.
    /// </summary>
    [JsonPropertyName("former_sponsor")]
    public string? FormerSponsor { get; init; }

    /// <summary>
    ///     The account ID of the new sponsor.
    /// </summary>
    [JsonPropertyName("new_sponsor")]
    public string? NewSponsor { get; init; }
}