using System;
using System.Net.Http;
using System.Threading.Tasks;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

public class
    LiquidityPoolsRequestBuilder : RequestBuilderStreamable<LiquidityPoolsRequestBuilder, LiquidityPoolResponse>
{
    public const string ReservesParameterName = "reserves";

    public LiquidityPoolsRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "liquidity_pools", httpClient)
    {
    }

    public async Task<LiquidityPoolResponse> LiquidityPool(Uri uri)
    {
        var responseHandler = new ResponseHandler<LiquidityPoolResponse>();

        var response = await HttpClient.GetAsync(uri);
        return await responseHandler.HandleResponse(response);
    }

    public async Task<LiquidityPoolResponse> LiquidityPool(string liquidityPoolId)
    {
        SetSegments("liquidity_pools", liquidityPoolId);
        return await LiquidityPool(BuildUri());
    }

    public async Task<LiquidityPoolResponse> LiquidityPool(LiquidityPoolId liquidityPoolId)
    {
        return await LiquidityPool(liquidityPoolId.ToString());
    }

    public LiquidityPoolsRequestBuilder ForReserves(params string[] reserves)
    {
        UriBuilder.SetQueryParam(ReservesParameterName, string.Join(",", reserves));
        return this;
    }
}