using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Predicates;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the claimable_balance_claimant_created effect response.
///     This effect occurs when a claimant is added to a claimable balance.
/// </summary>
public sealed class ClaimableBalanceClaimantCreatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 51;

    /// <summary>
    ///     The asset held in the claimable balance (in canonical form: "code:issuer" or "native").
    /// </summary>
    [JsonPropertyName("asset")]
    public string? Asset { get; init; }

    /// <summary>
    ///     The unique identifier of the claimable balance.
    /// </summary>
    [JsonPropertyName("balance_id")]
    public string? BalanceId { get; init; }

    /// <summary>
    ///     The amount of the asset in the claimable balance.
    /// </summary>
    [JsonPropertyName("amount")]
    public string? Amount { get; init; }

    /// <summary>
    ///     The condition that must be satisfied for the balance to be claimed.
    /// </summary>
    [JsonPropertyName("predicate")]
    public Predicate? Predicate { get; set; }
}