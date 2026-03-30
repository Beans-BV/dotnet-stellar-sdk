using System;
using System.Net.Http;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Builds requests connected to trades.
/// </summary>
public class TradesRequestBuilder : RequestBuilderExecutePageable<TradesRequestBuilder, TradeResponse>
{
    public TradesRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "trades", httpClient)
    {
    }

    /// <summary>
    ///     Filters trades by the specified base asset.
    /// </summary>
    /// <param name="asset">The base asset to filter trades by.</param>
    /// <returns>The current <see cref="TradesRequestBuilder" /> instance for chaining.</returns>
    public TradesRequestBuilder BaseAsset(Asset asset)
    {
        UriBuilder.SetQueryParam("base_asset_type", asset.Type);
        if (asset is AssetTypeCreditAlphaNum creditAlphaNumAsset)
        {
            UriBuilder.SetQueryParam("base_asset_code", creditAlphaNumAsset.Code);
            UriBuilder.SetQueryParam("base_asset_issuer", creditAlphaNumAsset.Issuer);
        }

        return this;
    }

    /// <summary>
    ///     Filters trades by the specified offer ID.
    /// </summary>
    /// <param name="offerId">The offer ID to filter trades by.</param>
    /// <returns>The current <see cref="TradesRequestBuilder" /> instance for chaining.</returns>
    public TradesRequestBuilder OfferId(string offerId)
    {
        UriBuilder.SetQueryParam("offer_id", offerId);
        return this;
    }

    /// <summary>
    ///     Filters trades by the specified counter asset.
    /// </summary>
    /// <param name="asset">The counter asset to filter trades by.</param>
    /// <returns>The current <see cref="TradesRequestBuilder" /> instance for chaining.</returns>
    public TradesRequestBuilder CounterAsset(Asset asset)
    {
        UriBuilder.SetQueryParam("counter_asset_type", asset.Type);
        if (asset is AssetTypeCreditAlphaNum creditAlphaNumAsset)
        {
            UriBuilder.SetQueryParam("counter_asset_code", creditAlphaNumAsset.Code);
            UriBuilder.SetQueryParam("counter_asset_issuer", creditAlphaNumAsset.Issuer);
        }

        return this;
    }


    /// <Summary>
    ///     Builds request to <code>GET /accounts/{account}/trades</code>
    ///     <a href="https://www.stellar.org/developers/horizon/reference/endpoints/trades-for-account.html">Trades for Account</a>
    /// </Summary>
    /// <param name="account">Account for which to get trades</param>
    public TradesRequestBuilder ForAccount(string accountId)
    {
        if (accountId is null)
        {
            throw new ArgumentNullException(nameof(accountId), "accountId cannot be null");
        }
        SetSegments("accounts", accountId, "trades");
        return this;
    }

    /// <summary>
    ///     Builds request to <code>GET /liquidity_pools/{liquidity_pool_id}/trades</code>
    ///     <a href="https://developers.stellar.org/docs/data/apis/horizon/api-reference/retrieve-related-trades">
    ///         Trades for
    ///         Liquidity Pool
    ///     </a>
    /// </summary>
    /// <param name="liquidityPoolId">Liquidity pool for which to get trades</param>
    public TradesRequestBuilder ForLiquidityPool(string liquidityPoolId)
    {
        if (liquidityPoolId is null)
        {
            throw new ArgumentNullException(nameof(liquidityPoolId), "liquidityPoolId cannot be null");
        }
        SetSegments("liquidity_pools", liquidityPoolId, "trades");
        return this;
    }

    /// <summary>
    ///     Builds request to <code>GET /offers/{offer_id}/trades</code>
    ///     <a href="https://developers.stellar.org/docs/data/apis/horizon/api-reference/get-trades-by-offer-id">Trades for Offer</a>
    /// </summary>
    /// <param name="offerId">Offer for which to get trades</param>
    public TradesRequestBuilder ForOffer(string offerId)
    {
        if (offerId is null)
        {
            throw new ArgumentNullException(nameof(offerId), "offerId cannot be null");
        }
        SetSegments("offers", offerId, "trades");
        return this;
    }
}