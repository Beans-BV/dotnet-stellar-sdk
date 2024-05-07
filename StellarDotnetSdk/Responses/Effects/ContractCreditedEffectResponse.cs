using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public class ContractCreditedEffectResponse : EffectResponse
{
    // TODO Find out which TypeId and add tests
    // public override int TypeId => ;

    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; init; }

    [JsonProperty(PropertyName = "asset_code")]
    public string AssetCode { get; init; }

    [JsonProperty(PropertyName = "asset_issuer")]
    public string AssetIssuer { get; init; }

    [JsonProperty(PropertyName = "asset_type")]
    public string AssetType { get; init; }

    [JsonProperty(PropertyName = "contract")]
    public string Contract { get; init; }
}