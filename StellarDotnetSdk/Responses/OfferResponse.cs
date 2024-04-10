using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Responses;
#nullable disable

/// <summary>
///     Represents offer response.
/// </summary>
public class OfferResponse : Response, IPagingToken
{
    [JsonProperty(PropertyName = "id")] public string Id { get; init; }

    [JsonProperty(PropertyName = "seller")]
    public string Seller { get; init; }

    [JsonProperty(PropertyName = "selling")]
    [JsonConverter(typeof(AssetJsonConverter))]
    public Asset Selling { get; init; }

    [JsonProperty(PropertyName = "buying")]
    [JsonConverter(typeof(AssetJsonConverter))]
    public Asset Buying { get; init; }

    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; init; }

    /// <summary>
    ///     The ask/bid price as a ratio.
    /// </summary>
    [JsonProperty(PropertyName = "price_r")]
    public Price PriceRatio { get; init; }

    /// <summary>
    ///     The ask/bid price.
    /// </summary>
    [JsonProperty(PropertyName = "price")]
    public string Price { get; init; }

    [JsonProperty(PropertyName = "last_modified_ledger")]
    public int LastModifiedLedger { get; init; }

    [JsonProperty(PropertyName = "last_modified_time")]
    public string LastModifiedTime { get; init; }

    [JsonProperty(PropertyName = "_links")]
    public OfferResponseLinks Links { get; init; }

    [JsonProperty(PropertyName = "paging_token")]
    public string PagingToken { get; init; }

    public class OfferResponseLinks
    {
        [JsonProperty(PropertyName = "self")] public Link<OfferResponse> Self { get; init; }

        [JsonProperty(PropertyName = "offer_maker")]
        public Link<AccountResponse> OfferMaker { get; init; }
    }
}