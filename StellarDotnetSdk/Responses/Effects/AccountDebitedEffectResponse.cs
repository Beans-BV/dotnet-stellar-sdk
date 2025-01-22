using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents account_debited effect response.
/// </summary>
public class AccountDebitedEffectResponse : EffectResponse
{
    public override int TypeId => 3;

    [JsonPropertyName("amount")]
    public string Amount { get; init; }

    [JsonPropertyName("asset_type")]
    public string AssetType { get; init; }

    [JsonPropertyName("asset_code")]
    public string AssetCode { get; init; }

    [JsonPropertyName("asset_issuer")]
    public string AssetIssuer { get; init; }

    public Asset Asset => Asset.Create(AssetType, AssetCode, AssetIssuer);
}