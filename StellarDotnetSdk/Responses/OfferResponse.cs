﻿using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents offer response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/offer.html
///     <seealso cref="OffersRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class OfferResponse : Response, IPagingToken
{
    public OfferResponse(string id, string pagingToken, string seller, Asset selling, Asset buying, string amount,
        string price, int lastModifiedLedger, string lastModifiedTime, OfferResponseLinks links, Price priceRatio)
    {
        Id = id;
        PagingToken = pagingToken;
        Seller = seller;
        Selling = selling;
        Buying = buying;
        Amount = amount;
        Price = price;
        LastModifiedLedger = lastModifiedLedger;
        LastModifiedTime = lastModifiedTime;
        Links = links;
        PriceRatio = priceRatio;
    }

    [JsonProperty(PropertyName = "id")] public string Id { get; private set; }

    [JsonProperty(PropertyName = "seller")]
    public string Seller { get; private set; }

    [JsonProperty(PropertyName = "selling")]
    [JsonConverter(typeof(AssetDeserializer))]
    public Asset Selling { get; private set; }

    [JsonProperty(PropertyName = "buying")]
    [JsonConverter(typeof(AssetDeserializer))]
    public Asset Buying { get; private set; }

    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; private set; }

    /// <summary>
    ///     The ask/bid price as a ratio.
    /// </summary>
    [JsonProperty(PropertyName = "price_r")]
    public Price PriceRatio { get; private set; }

    /// <summary>
    ///     The ask/bid price.
    /// </summary>
    [JsonProperty(PropertyName = "price")]
    public string Price { get; private set; }

    [JsonProperty(PropertyName = "last_modified_ledger")]
    public int LastModifiedLedger { get; private set; }

    [JsonProperty(PropertyName = "last_modified_time")]
    public string LastModifiedTime { get; private set; }

    [JsonProperty(PropertyName = "_links")]
    public OfferResponseLinks Links { get; private set; }

    [JsonProperty(PropertyName = "paging_token")]
    public string PagingToken { get; private set; }
}