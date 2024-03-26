using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.effects;

/// <summary>
///     Represents claimable_balance_created effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class ClaimableBalanceCreatedEffectResponse : EffectResponse
{
    public ClaimableBalanceCreatedEffectResponse()
    {
    }

    public ClaimableBalanceCreatedEffectResponse(string asset, string balanceID, string amount)
    {
        Asset = asset;
        BalanceID = balanceID;
        Amount = amount;
    }

    public override int TypeId => 50;

    [JsonProperty(PropertyName = "asset")] public string Asset { get; private set; }

    [JsonProperty(PropertyName = "balance_id")]
    public string BalanceID { get; private set; }

    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; private set; }
}