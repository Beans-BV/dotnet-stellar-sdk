using System.Collections.Generic;
using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class PathResponse : Response
{
    [JsonProperty(PropertyName = "destination_amount")]
    public string DestinationAmount { get; init; }

    [JsonProperty(PropertyName = "destination_asset_type")]
    public string DestinationAssetType { get; init; }

    [JsonProperty(PropertyName = "destination_asset_code")]
    public string DestinationAssetCode { get; init; }

    [JsonProperty(PropertyName = "destination_asset_issuer")]
    public string DestinationAssetIssuer { get; init; }

    [JsonProperty(PropertyName = "source_amount")]
    public string SourceAmount { get; init; }

    [JsonProperty(PropertyName = "source_asset_type")]
    public string SourceAssetType { get; init; }

    [JsonProperty(PropertyName = "source_asset_code")]
    public string SourceAssetCode { get; init; }

    [JsonProperty(PropertyName = "source_asset_issuer")]
    public string SourceAssetIssuer { get; init; }

    [JsonProperty(PropertyName = "path", ItemConverterType = typeof(AssetJsonConverter))]
    public List<Asset> Path { get; init; }

    [JsonProperty(PropertyName = "_links")]
    public PathResponseLinks Links { get; init; }

    public Asset DestinationAsset => Asset.Create(DestinationAssetType, DestinationAssetCode, DestinationAssetIssuer);

    public Asset SourceAsset => Asset.Create(SourceAssetType, SourceAssetCode, SourceAssetIssuer);

    public class PathResponseLinks
    {
        [JsonProperty(PropertyName = "self")] public Link<PathResponse> Self { get; init; }
    }
}