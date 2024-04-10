using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents claimable_balance_created effect response.
/// </summary>
public class ClaimableBalanceClaimantCreatedEffectResponse : EffectResponse
{
    public override int TypeId => 51;

    [JsonProperty(PropertyName = "asset")] public string Asset { get; init; }

    [JsonProperty(PropertyName = "balance_id")]
    public string BalanceId { get; init; }

    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; init; }

    [JsonProperty(PropertyName = "predicate")]
    public Predicate Predicate { get; set; }
}