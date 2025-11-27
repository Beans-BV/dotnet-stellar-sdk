using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class PathResponse : Response
{
    [JsonPropertyName("destination_amount")]
    public string DestinationAmount { get; init; }

    [JsonPropertyName("destination_asset_type")]
    public string DestinationAssetType { get; init; }

    [JsonPropertyName("destination_asset_code")]
    public string DestinationAssetCode { get; init; }

    [JsonPropertyName("destination_asset_issuer")]
    public string DestinationAssetIssuer { get; init; }

    [JsonPropertyName("source_amount")]
    public string SourceAmount { get; init; }

    [JsonPropertyName("source_asset_type")]
    public string SourceAssetType { get; init; }

    [JsonPropertyName("source_asset_code")]
    public string SourceAssetCode { get; init; }

    [JsonPropertyName("source_asset_issuer")]
    public string SourceAssetIssuer { get; init; }

    [JsonPropertyName("path")]
    public List<Asset> Path { get; init; }

    [JsonPropertyName("_links")]
    public PathResponseLinks Links { get; init; }

    public Asset DestinationAsset => Asset.Create(DestinationAssetType, DestinationAssetCode, DestinationAssetIssuer);

    public Asset SourceAsset => Asset.Create(SourceAssetType, SourceAssetCode, SourceAssetIssuer);

    public class PathResponseLinks
    {
        [JsonPropertyName("self")] public Link<PathResponse> Self { get; init; }
    }
}