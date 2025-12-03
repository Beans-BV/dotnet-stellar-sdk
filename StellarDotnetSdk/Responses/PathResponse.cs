using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a payment path found by the path finding algorithm.
///     A path is a sequence of assets that can be used to convert from a source asset
///     to a destination asset through the Stellar decentralized exchange.
/// </summary>
public sealed class PathResponse : Response
{
    /// <summary>
    ///     The amount of the destination asset that will be received.
    ///     Represented as a string to preserve precision.
    /// </summary>
    [JsonPropertyName("destination_amount")]
    public required string DestinationAmount { get; init; }

    /// <summary>
    ///     The type of the destination asset: "native", "credit_alphanum4", or "credit_alphanum12".
    /// </summary>
    [JsonPropertyName("destination_asset_type")]
    public required string DestinationAssetType { get; init; }

    /// <summary>
    ///     The code of the destination asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("destination_asset_code")]
    public string? DestinationAssetCode { get; init; }

    /// <summary>
    ///     The issuer of the destination asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("destination_asset_issuer")]
    public string? DestinationAssetIssuer { get; init; }

    /// <summary>
    ///     The amount of the source asset required for this path.
    ///     Represented as a string to preserve precision.
    /// </summary>
    [JsonPropertyName("source_amount")]
    public required string SourceAmount { get; init; }

    /// <summary>
    ///     The type of the source asset: "native", "credit_alphanum4", or "credit_alphanum12".
    /// </summary>
    [JsonPropertyName("source_asset_type")]
    public required string SourceAssetType { get; init; }

    /// <summary>
    ///     The code of the source asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("source_asset_code")]
    public string? SourceAssetCode { get; init; }

    /// <summary>
    ///     The issuer of the source asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("source_asset_issuer")]
    public string? SourceAssetIssuer { get; init; }

    /// <summary>
    ///     The list of intermediate assets in this path (excluding source and destination).
    ///     Can be empty if this is a direct exchange.
    /// </summary>
    [JsonPropertyName("path")]
    public required IReadOnlyList<Asset> Path { get; init; }

    /// <summary>
    ///     The destination asset object.
    /// </summary>
    public Asset DestinationAsset => Asset.Create(DestinationAssetType, DestinationAssetCode, DestinationAssetIssuer);

    /// <summary>
    ///     The source asset object.
    /// </summary>
    public Asset SourceAsset => Asset.Create(SourceAssetType, SourceAssetCode, SourceAssetIssuer);
}