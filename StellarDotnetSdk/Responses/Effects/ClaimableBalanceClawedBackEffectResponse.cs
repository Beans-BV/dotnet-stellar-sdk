using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents claimable_balance_clawed_back effect response.
/// </summary>
public class ClaimableBalanceClawedBackEffectResponse : EffectResponse
{
    public override int TypeId => 80;

    [JsonProperty(PropertyName = "balance_id")]
    public string BalanceId { get; init; }
}