using System;
using System.Net.Http;
using System.Threading.Tasks;
using stellar_dotnet_sdk.responses;

namespace stellar_dotnet_sdk.requests;

public class FeeStatsRequestBuilder : RequestBuilder<FeeStatsRequestBuilder>
{
    public FeeStatsRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "fee_stats", httpClient)
    {
    }

    public async Task<FeeStatsResponse> Execute()
    {
        return await Execute<FeeStatsResponse>(BuildUri());
    }
}