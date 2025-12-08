using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the claimable_balance_sponsorship_removed effect response.
///     This effect occurs when a claimable balance's sponsorship is removed.
/// </summary>
public sealed class ClaimableBalanceSponsorshipRemovedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 71;

    /// <summary>
    ///     The unique identifier of the claimable balance.
    /// </summary>
    [JsonPropertyName("balance_id")]
    public required string BalanceId { get; init; }

    /// <summary>
    ///     The account ID of the former sponsor.
    /// </summary>
    [JsonPropertyName("former_sponsor")]
    public required string FormerSponsor { get; init; }
}