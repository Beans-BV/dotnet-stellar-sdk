using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents claimable_balance_created effect response.
/// </summary>
public class ClaimableBalanceCreatedEffectResponse : EffectResponse
{
    public override int TypeId => 50;

    [JsonProperty(PropertyName = "asset")] public string Asset { get; init; }

    [JsonProperty(PropertyName = "balance_id")]
    public string BalanceId { get; init; }

    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; init; }
}