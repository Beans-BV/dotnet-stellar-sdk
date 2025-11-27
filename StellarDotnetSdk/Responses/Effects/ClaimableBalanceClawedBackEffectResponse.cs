using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the claimable_balance_clawed_back effect response.
///     This effect occurs when a claimable balance is clawed back by the issuer.
/// </summary>
public sealed class ClaimableBalanceClawedBackEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 80;

    /// <summary>
    ///     The unique identifier of the claimable balance that was clawed back.
    /// </summary>
    [JsonPropertyName("balance_id")]
    public string? BalanceId { get; init; }
}