using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the claimable_balance_sponsorship_created effect response.
///     This effect occurs when a claimable balance becomes sponsored.
/// </summary>
public sealed class ClaimableBalanceSponsorshipCreatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 69;

    /// <summary>
    ///     The unique identifier of the sponsored claimable balance.
    /// </summary>
    [JsonPropertyName("balance_id")]
    public required string BalanceId { get; init; }

    /// <summary>
    ///     The account ID of the sponsor.
    /// </summary>
    [JsonPropertyName("sponsor")]
    public required string Sponsor { get; init; }
}