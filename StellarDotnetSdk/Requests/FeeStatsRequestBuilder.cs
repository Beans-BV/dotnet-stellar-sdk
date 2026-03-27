using System;
using System.Net.Http;
using System.Threading.Tasks;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Builds requests to the Horizon <c>/fee_stats</c> endpoint, which provides information about
///     per-operation fee statistics over the last 5 ledgers.
/// </summary>
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