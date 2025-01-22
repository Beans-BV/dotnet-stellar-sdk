using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents claimable_balance_created effect response.
/// </summary>
public class ClaimableBalanceCreatedEffectResponse : EffectResponse
{
    public override int TypeId => 50;

    [JsonPropertyName("asset")] public string Asset { get; init; }

    [JsonPropertyName("balance_id")]
    public string BalanceId { get; init; }

    [JsonPropertyName("amount")]
    public string Amount { get; init; }
}