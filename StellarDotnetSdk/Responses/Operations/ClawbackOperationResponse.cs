using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <inheritdoc />
/// <summary>
///     Represents Clawback operation response.
/// </summary>
public class ClawbackOperationResponse : OperationResponse
{
    public override int TypeId => 19;

    /// <summary>
    ///     Asset type (native / alphanum4 / alphanum12)
    /// </summary>
    [JsonPropertyName("asset_type")]
    public string AssetType { get; init; }

    /// <summary>
    ///     Asset code.
    /// </summary>
    [JsonPropertyName("asset_code")]
    public string AssetCode { get; init; }

    /// <summary>
    ///     Asset issuer.
    /// </summary>
    [JsonPropertyName("asset_issuer")]
    public string AssetIssuer { get; init; }

    /// <summary>
    ///     Amount
    /// </summary>
    [JsonPropertyName("amount")]
    public string Amount { get; init; }

    /// <summary>
    ///     Account from which the asset is clawed back
    /// </summary>
    [JsonPropertyName("from")]
    public string From { get; init; }

    /// <summary>
    ///     Muxed Account from which the asset is clawed back
    /// </summary>
    [JsonPropertyName("from_muxed")]
    public string FromMuxed { get; init; }

    /// <summary>
    ///     Muxed Account ID from which the asset is clawed back
    /// </summary>
    [JsonPropertyName("from_muxed_id")]
    public ulong? FromMuxedID { get; init; }

    /// <summary>
    ///     Asset representation (Using the values of the other fields)
    /// </summary>
    public AssetTypeCreditAlphaNum Asset => Assets.Asset.CreateNonNativeAsset(AssetCode, AssetIssuer);
}