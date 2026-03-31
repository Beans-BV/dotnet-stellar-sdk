using System;
using System.Net.Http;
using System.Threading.Tasks;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Builds requests to the Horizon <c>/liquidity_pools</c> endpoint for retrieving
///     liquidity pool details and streaming pool events.
/// </summary>
public class
    LiquidityPoolsRequestBuilder : RequestBuilderStreamable<LiquidityPoolsRequestBuilder, LiquidityPoolResponse>
{
    /// <summary>
    ///     The query parameter name used to filter liquidity pools by reserve assets.
    /// </summary>
    public const string ReservesParameterName = "reserves";

    /// <summary>
    ///     Initializes a new <see cref="LiquidityPoolsRequestBuilder" />.
    /// </summary>
    /// <param name="serverUri">The base Horizon server URI.</param>
    /// <param name="httpClient">The HTTP client used for sending requests.</param>
    public LiquidityPoolsRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "liquidity_pools", httpClient)
    {
    }

    /// <summary>
    ///     Requests a single liquidity pool resource from the specified URI.
    /// </summary>
    /// <param name="uri">The URI of the liquidity pool resource.</param>
    /// <returns>The <see cref="LiquidityPoolResponse" />.</returns>
    public async Task<LiquidityPoolResponse> LiquidityPool(Uri uri)
    {
        var responseHandler = new ResponseHandler<LiquidityPoolResponse>();

        var response = await HttpClient.GetAsync(uri);
        return await responseHandler.HandleResponse(response);
    }

    /// <summary>
    ///     Requests <c>GET /liquidity_pools/{liquidityPoolId}</c>.
    /// </summary>
    /// <param name="liquidityPoolId">The ID of the liquidity pool to fetch.</param>
    /// <returns>The <see cref="LiquidityPoolResponse" />.</returns>
    public async Task<LiquidityPoolResponse> LiquidityPool(string liquidityPoolId)
    {
        SetSegments("liquidity_pools", liquidityPoolId);
        return await LiquidityPool(BuildUri());
    }

    /// <summary>
    ///     Requests <c>GET /liquidity_pools/{liquidityPoolId}</c>.
    /// </summary>
    /// <param name="liquidityPoolId">The <see cref="LiquidityPool.LiquidityPoolId" /> of the pool to fetch.</param>
    /// <returns>The <see cref="LiquidityPoolResponse" />.</returns>
    public async Task<LiquidityPoolResponse> LiquidityPool(LiquidityPoolId liquidityPoolId)
    {
        return await LiquidityPool(liquidityPoolId.ToString());
    }

    /// <summary>
    ///     Filters liquidity pools that have reserves matching all of the specified assets.
    /// </summary>
    /// <param name="reserves">
    ///     The canonical asset identifiers (e.g. <c>"native"</c> or <c>"USD:GABC..."</c>) to filter by.
    /// </param>
    /// <returns>The current <see cref="LiquidityPoolsRequestBuilder" /> instance for chaining.</returns>
    public LiquidityPoolsRequestBuilder ForReserves(params string[] reserves)
    {
        UriBuilder.SetQueryParam(ReservesParameterName, string.Join(",", reserves));
        return this;
    }
}