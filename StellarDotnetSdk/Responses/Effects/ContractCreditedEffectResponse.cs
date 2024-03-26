using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

public class ContractCreditedEffectResponse : EffectResponse
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

    public ContractCreditedEffectResponse(string amount, string assetCode, string assetIssuer, string assetType,
        string contract)
    {
        Amount = amount;
        AssetCode = assetCode;
        AssetIssuer = assetIssuer;
        AssetType = assetType;
        Contract = contract;
    }
}