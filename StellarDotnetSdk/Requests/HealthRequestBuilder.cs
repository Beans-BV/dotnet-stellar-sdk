using System;
using System.Net.Http;
using System.Threading.Tasks;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Builds requests connected to the Horizon health endpoint.
/// </summary>
public class HealthRequestBuilder : RequestBuilder<HealthRequestBuilder>
{
    public HealthRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "health", httpClient)
    {
    }

    /// <summary>
    ///     Requests <code>GET /health</code>
    ///     <a href="https://developers.stellar.org/docs/data/horizon/api-reference/aggregations/health">Health</a>
    /// </summary>
    public async Task<HealthResponse> Execute()
    {
        return await Execute<HealthResponse>(BuildUri());
    }
}