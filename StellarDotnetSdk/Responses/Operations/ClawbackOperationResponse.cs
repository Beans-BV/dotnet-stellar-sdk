using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a clawback operation response.
///     Claws back (burns) a specified amount of an asset from an account.
///     This operation can only be performed by the asset issuer on assets that have the clawback-enabled flag set.
///     The clawed back amount is permanently removed from circulation.
/// </summary>
public class ClawbackOperationResponse : OperationResponse
{
    public override int TypeId => 19;

    /// <summary>
    ///     The type of asset being clawed back (e.g., "credit_alphanum4", "credit_alphanum12").
    ///     Note: Native XLM cannot be clawed back.
    /// </summary>
    [JsonPropertyName("asset_type")]
    public required string AssetType { get; init; }

    /// <summary>
    ///     The asset code of the asset being clawed back (e.g., "USD", "BTC").
    /// </summary>
    [JsonPropertyName("asset_code")]
    public required string AssetCode { get; init; }

    /// <summary>
    ///     The account that issued the asset being clawed back.
    /// </summary>
    [JsonPropertyName("asset_issuer")]
    public required string AssetIssuer { get; init; }

    /// <summary>
    ///     The amount of the asset being clawed back.
    /// </summary>
    [JsonPropertyName("amount")]
    public required string Amount { get; init; }

    /// <summary>
    ///     The account address from which the asset is being clawed back.
    /// </summary>
    [JsonPropertyName("from")]
    public required string From { get; init; }

    /// <summary>
    ///     The muxed account representation of the account from which the asset is being clawed back, if applicable.
    /// </summary>
    [JsonPropertyName("from_muxed")]
    public string? FromMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the account from which the asset is being clawed back, if applicable.
    /// </summary>
    [JsonPropertyName("from_muxed_id")]
    public ulong? FromMuxedId { get; init; }

    /// <summary>
    ///     The asset being clawed back, constructed from the asset type, code, and issuer.
    /// </summary>
    [JsonIgnore]
    public Asset Asset => Asset.CreateNonNativeAsset(AssetCode, AssetIssuer);
}