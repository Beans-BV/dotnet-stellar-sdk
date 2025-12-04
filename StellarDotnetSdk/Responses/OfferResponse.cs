using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents offer response.
/// </summary>
public class OfferResponse : Response, IPagingToken
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("seller")]
    public string Seller { get; init; }

    [JsonPropertyName("selling")]
    [JsonConverter(typeof(AssetJsonConverter))]
    public Asset Selling { get; init; }

    [JsonPropertyName("buying")]
    [JsonConverter(typeof(AssetJsonConverter))]
    public Asset Buying { get; init; }

    [JsonPropertyName("amount")]
    public string Amount { get; init; }

    /// <summary>
    ///     The ask/bid price as a ratio.
    /// </summary>
    [JsonPropertyName("price_r")]
    public StellarDotnetSdk.Price PriceRatio { get; init; }

    /// <summary>
    ///     The ask/bid price.
    /// </summary>
    [JsonPropertyName("price")]
    public string Price { get; init; }

    [JsonPropertyName("last_modified_ledger")]
    public int LastModifiedLedger { get; init; }

    [JsonPropertyName("last_modified_time")]
    public DateTimeOffset? LastModifiedTime { get; init; }

    [JsonPropertyName("_links")]
    public OfferResponseLinks Links { get; init; }

    [JsonPropertyName("paging_token")]
    public string PagingToken { get; init; }

    public class OfferResponseLinks
    {
        [JsonPropertyName("self")]
        public Link<OfferResponse> Self { get; init; }

        [JsonPropertyName("offer_maker")]
        public Link<AccountResponse> OfferMaker { get; init; }
    }
}