using Newtonsoft.Json;
using stellar_dotnet_sdk.converters;

namespace stellar_dotnet_sdk.responses.effects;

[JsonConverter(typeof(LiquidityPoolClaimableAssetAmountJsonConverter))]
public class LiquidityPoolClaimableAssetAmount
{
    public LiquidityPoolClaimableAssetAmount(Asset asset, string amount, string claimableBalanceID)
    {
        Asset = asset;
        Amount = amount;
        ClaimableBalanceID = claimableBalanceID;
    }

    [JsonProperty(PropertyName = "asset")] public Asset Asset { get; }

    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; }

    [JsonProperty(PropertyName = "claimable_balance_id")]
    public string ClaimableBalanceID { get; } // TODO: Check if nullable
}