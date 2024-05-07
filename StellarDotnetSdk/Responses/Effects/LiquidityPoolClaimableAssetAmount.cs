using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

[JsonConverter(typeof(LiquidityPoolClaimableAssetAmountJsonConverter))]
public class LiquidityPoolClaimableAssetAmount
{
    [JsonProperty(PropertyName = "asset")] public Asset Asset { get; init; }

    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; init; }

    [JsonProperty(PropertyName = "claimable_balance_id")]
    public string ClaimableBalanceId { get; init; } // TODO: Check if nullable
}