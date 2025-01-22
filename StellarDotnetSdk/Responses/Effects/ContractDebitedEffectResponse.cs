using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public class ContractDebitedEffectResponse : EffectResponse
{
    // TODO Find out which TypeId and add tests
    // public override int TypeId => ;

    [JsonPropertyName("amount")]
    public string Amount { get; init; }

    [JsonPropertyName("asset_code")]
    public string AssetCode { get; init; }

    [JsonPropertyName("asset_issuer")]
    public string AssetIssuer { get; init; }

    [JsonPropertyName("asset_type")]
    public string AssetType { get; init; }

    [JsonPropertyName("contract")]
    public string Contract { get; init; }
}