using System;
using System.Net.Http;
using System.Threading.Tasks;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Builds requests to the Horizon <c>/order_book</c> endpoint for retrieving
///     the current order book for a given asset pair.
/// </summary>
public class OrderBookRequestBuilder : RequestBuilder<OrderBookRequestBuilder>
{
    /// <summary>
    ///     Initializes a new <see cref="OrderBookRequestBuilder" />.
    /// </summary>
    /// <param name="serverUri">The base Horizon server URI.</param>
    /// <param name="httpClient">The HTTP client used for sending requests.</param>
    public OrderBookRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "order_book", httpClient)
    {
    }

    /// <summary>
    ///     Sets the asset being bought (the ask side of the order book).
    /// </summary>
    /// <param name="asset">The asset being bought.</param>
    /// <returns>The current <see cref="OrderBookRequestBuilder" /> instance for chaining.</returns>
    public OrderBookRequestBuilder BuyingAsset(Asset asset)
    {
        UriBuilder.SetQueryParam("buying_asset_type", asset.Type);
        if (asset is AssetTypeCreditAlphaNum creditAlphaNumAsset)
        {
            UriBuilder.SetQueryParam("buying_asset_code", creditAlphaNumAsset.Code);
            UriBuilder.SetQueryParam("buying_asset_issuer", creditAlphaNumAsset.Issuer);
        }

        return this;
    }

    /// <summary>
    ///     Sets the asset being sold (the bid side of the order book).
    /// </summary>
    /// <param name="asset">The asset being sold.</param>
    /// <returns>The current <see cref="OrderBookRequestBuilder" /> instance for chaining.</returns>
    public OrderBookRequestBuilder SellingAsset(Asset asset)
    {
        UriBuilder.SetQueryParam("selling_asset_type", asset.Type);
        if (asset is AssetTypeCreditAlphaNum creditAlphaNumAsset)
        {
            UriBuilder.SetQueryParam("selling_asset_code", creditAlphaNumAsset.Code);
            UriBuilder.SetQueryParam("selling_asset_issuer", creditAlphaNumAsset.Issuer);
        }

        return this;
    }

    /// <summary>
    ///     Not supported for the order book endpoint.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown; order book does not support cursor pagination.</exception>
    public override OrderBookRequestBuilder Cursor(string token)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Not supported for the order book endpoint.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown; order book does not support ordering.</exception>
    public override OrderBookRequestBuilder Order(OrderDirection direction)
    {
        throw new NotImplementedException();
    }

    /// <Summary>
    ///     Build and execute request.
    /// </Summary>
    public async Task<OrderBookResponse> Execute()
    {
        return await Execute<OrderBookResponse>(BuildUri());
    }
}