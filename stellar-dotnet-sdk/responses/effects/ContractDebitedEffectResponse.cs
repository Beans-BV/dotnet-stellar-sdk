using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.effects;

public class ContractDebitedEffectResponse : EffectResponse
{
    [JsonProperty(PropertyName = "amount")]
    public string Amount;

    [JsonProperty(PropertyName = "asset_code")]
    public string AssetCode;

    [JsonProperty(PropertyName = "asset_issuer")]
    public string AssetIssuer;

    [JsonProperty(PropertyName = "asset_type")]
    public string AssetType;

    [JsonProperty(PropertyName = "contract")]
    public string Contract;
}