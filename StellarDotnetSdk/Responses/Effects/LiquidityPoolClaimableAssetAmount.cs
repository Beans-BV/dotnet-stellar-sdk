using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

[JsonConverter(typeof(LiquidityPoolClaimableAssetAmountJsonConverter))]
public class LiquidityPoolClaimableAssetAmount
{
    [JsonPropertyName("asset")] public Asset Asset { get; init; }

    [JsonPropertyName("amount")]
    public string Amount { get; init; }

    [JsonPropertyName("claimable_balance_id")]
    public string ClaimableBalanceId { get; init; } // TODO: Check if nullable
}