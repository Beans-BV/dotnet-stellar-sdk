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
    /// <summary>
    ///     Initializes a new <see cref="FeeStatsRequestBuilder" />.
    /// </summary>
    /// <param name="serverUri">The base Horizon server URI.</param>
    /// <param name="httpClient">The HTTP client used for sending requests.</param>
    public FeeStatsRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "fee_stats", httpClient)
    {
    }

    /// <summary>
    ///     Executes the request to <c>GET /fee_stats</c> and returns the fee statistics response.
    /// </summary>
    /// <returns>The <see cref="FeeStatsResponse" /> containing fee statistics.</returns>
    public async Task<FeeStatsResponse> Execute()
    {
        return await Execute<FeeStatsResponse>(BuildUri());
    }
}