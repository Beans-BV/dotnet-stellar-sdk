using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a path_payment_strict_send operation response.
///     Sends an amount in a specific asset to a destination account through a path of offers.
///     The send amount is specified, and the receive amount can vary.
/// </summary>
public class PathPaymentStrictSendOperationResponse : OperationResponse
{
    public override int TypeId => 13;

    /// <summary>
    ///     The account address that sent the payment.
    /// </summary>
    [JsonPropertyName("from")]
    public required string From { get; init; }

    /// <summary>
    ///     The muxed account representation of the sender, if applicable.
    /// </summary>
    [JsonPropertyName("from_muxed")]
    public string? FromMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the sender, if applicable.
    /// </summary>
    [JsonPropertyName("from_muxed_id")]
    public ulong? FromMuxedId { get; init; }

    /// <summary>
    ///     The account address that received the payment.
    /// </summary>
    [JsonPropertyName("to")]
    public required string To { get; init; }

    /// <summary>
    ///     The muxed account representation of the receiver, if applicable.
    /// </summary>
    [JsonPropertyName("to_muxed")]
    public string? ToMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the receiver, if applicable.
    /// </summary>
    [JsonPropertyName("to_muxed_id")]
    public ulong? ToMuxedId { get; init; }

    /// <summary>
    ///     The type of destination asset (e.g., "native", "credit_alphanum4", "credit_alphanum12").
    /// </summary>
    [JsonPropertyName("asset_type")]
    public required string AssetType { get; init; }

    /// <summary>
    ///     The destination asset code. Only present for non-native assets.
    /// </summary>
    [JsonPropertyName("asset_code")]
    public string? AssetCode { get; init; }

    /// <summary>
    ///     The destination asset issuer account. Only present for non-native assets.
    /// </summary>
    [JsonPropertyName("asset_issuer")]
    public string? AssetIssuer { get; init; }

    /// <summary>
    ///     The amount of destination asset the destination account received.
    /// </summary>
    [JsonPropertyName("amount")]
    public required string Amount { get; init; }

    /// <summary>
    ///     The type of source asset (e.g., "native", "credit_alphanum4", "credit_alphanum12").
    /// </summary>
    [JsonPropertyName("source_asset_type")]
    public required string SourceAssetType { get; init; }

    /// <summary>
    ///     The source asset code. Only present for non-native assets.
    /// </summary>
    [JsonPropertyName("source_asset_code")]
    public string? SourceAssetCode { get; init; }

    /// <summary>
    ///     The source asset issuer account. Only present for non-native assets.
    /// </summary>
    [JsonPropertyName("source_asset_issuer")]
    public string? SourceAssetIssuer { get; init; }

    /// <summary>
    ///     The amount of the source asset that was sent.
    /// </summary>
    [JsonPropertyName("source_amount")]
    public required string SourceAmount { get; init; }

    /// <summary>
    ///     The minimum amount of the destination asset that can be received.
    /// </summary>
    [JsonPropertyName("destination_min")]
    public required string DestinationMin { get; init; }

    /// <summary>
    ///     The assets (excluding source and destination) the payment path goes through.
    /// </summary>
    [JsonPropertyName("path")]
    public required Asset[] Path { get; init; }

    /// <summary>
    ///     The destination asset.
    /// </summary>
    [JsonIgnore]
    public Asset DestinationAsset => Asset.Create(AssetType, AssetCode, AssetIssuer);

    /// <summary>
    ///     The source asset.
    /// </summary>
    [JsonIgnore]
    public Asset SourceAsset => Asset.Create(SourceAssetType, SourceAssetCode, SourceAssetIssuer);
}