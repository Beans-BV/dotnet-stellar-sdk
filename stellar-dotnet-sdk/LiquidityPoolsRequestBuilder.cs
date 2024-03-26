using System;
using System.Net.Http;
using System.Threading.Tasks;
using stellar_dotnet_sdk.requests;
using stellar_dotnet_sdk.responses;

namespace stellar_dotnet_sdk;

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

    public async Task<LiquidityPoolResponse> LiquidityPool(string liquidityPoolID)
    {
        SetSegments("liquidity_pools", liquidityPoolID);
        return await LiquidityPool(BuildUri());
    }

    public async Task<LiquidityPoolResponse> LiquidityPool(LiquidityPoolID liquidityPoolID)
    {
        return await LiquidityPool(liquidityPoolID.ToString());
    }

    public LiquidityPoolsRequestBuilder ForReserves(params string[] reserves)
    {
        UriBuilder.SetQueryParam(ReservesParameterName, string.Join(",", reserves));
        return this;
    }
}