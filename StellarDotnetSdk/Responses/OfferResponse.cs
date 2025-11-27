using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents an offer on the Stellar decentralized exchange.
///     An offer is a bid or ask to exchange one asset for another at a specified price.
/// </summary>
public sealed class OfferResponse : Response, IPagingToken
{
    /// <summary>
    ///     The unique identifier for this offer.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    ///     The account ID of the offer creator.
    /// </summary>
    [JsonPropertyName("seller")]
    public required string Seller { get; init; }

    /// <summary>
    ///     The asset being sold.
    /// </summary>
    [JsonPropertyName("selling")]
    [JsonConverter(typeof(AssetJsonConverter))]
    public required Asset Selling { get; init; }

    /// <summary>
    ///     The asset being bought.
    /// </summary>
    [JsonPropertyName("buying")]
    [JsonConverter(typeof(AssetJsonConverter))]
    public required Asset Buying { get; init; }

    /// <summary>
    ///     The amount of the selling asset available for trade.
    ///     Represented as a string to preserve precision.
    /// </summary>
    [JsonPropertyName("amount")]
    public required string Amount { get; init; }

    /// <summary>
    ///     The ask/bid price as a ratio (numerator/denominator).
    ///     Represents how much of the buying asset is required for 1 unit of the selling asset.
    /// </summary>
    [JsonPropertyName("price_r")]
    public required Price PriceRatio { get; init; }

    /// <summary>
    ///     The ask/bid price as a decimal string.
    ///     Represents how much of the buying asset is required for 1 unit of the selling asset.
    /// </summary>
    [JsonPropertyName("price")]
    public required string Price { get; init; }

    /// <summary>
    ///     The sequence number of the last ledger in which this offer was modified.
    /// </summary>
    [JsonPropertyName("last_modified_ledger")]
    public required int LastModifiedLedger { get; init; }

    /// <summary>
    ///     An ISO 8601 formatted string of when this offer was last modified.
    /// </summary>
    [JsonPropertyName("last_modified_time")]
    public required DateTimeOffset? LastModifiedTime { get; init; }

    /// <summary>
    ///     The account id of the sponsor who is paying the reserves for this offer.
    /// </summary>
    [JsonPropertyName("sponsor")]
    public string? Sponsor { get; init; }

    /// <summary>
    ///     Links to related resources for this offer.
    /// </summary>
    [JsonPropertyName("_links")]
    public required OfferResponseLinks Links { get; init; }

    /// <inheritdoc />
    [JsonPropertyName("paging_token")]
    public required string PagingToken { get; init; }

    /// <summary>
    ///     Links to related resources for an offer.
    /// </summary>
    public sealed class OfferResponseLinks
    {
        /// <summary>
        ///     Link to this offer resource.
        /// </summary>
        [JsonPropertyName("self")]
        public required Link<OfferResponse> Self { get; init; }

        /// <summary>
        ///     Link to the account that created this offer.
        /// </summary>
        [JsonPropertyName("offer_maker")]
        public required Link<AccountResponse> OfferMaker { get; init; }
    }
}