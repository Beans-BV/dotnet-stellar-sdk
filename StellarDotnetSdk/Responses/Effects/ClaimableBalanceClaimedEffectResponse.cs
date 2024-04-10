using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents claimable_balance_claimed effect response.
/// </summary>
public class ClaimableBalanceClaimedEffectResponse : EffectResponse
{
    public override int TypeId => 52;

    [JsonProperty(PropertyName = "asset")] public string Asset { get; init; }

    [JsonProperty(PropertyName = "balance_id")]
    public string BalanceId { get; init; }

    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; init; }
}