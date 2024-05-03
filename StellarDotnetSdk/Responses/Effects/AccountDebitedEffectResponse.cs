using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents account_debited effect response.
/// </summary>
public class AccountDebitedEffectResponse : EffectResponse
{
    public override int TypeId => 3;

    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; init; }

    [JsonProperty(PropertyName = "asset_type")]
    public string AssetType { get; init; }

    [JsonProperty(PropertyName = "asset_code")]
    public string AssetCode { get; init; }

    [JsonProperty(PropertyName = "asset_issuer")]
    public string AssetIssuer { get; init; }

    public Asset Asset => Asset.Create(AssetType, AssetCode, AssetIssuer);
}