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
    /// <summary>
    ///     Initializes a new <see cref="HealthRequestBuilder" />.
    /// </summary>
    /// <param name="serverUri">The base Horizon server URI.</param>
    /// <param name="httpClient">The HTTP client used for sending requests.</param>
    public HealthRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "health", httpClient)
    {
    }

    /// <summary>
    ///     Requests <code>GET /health</code>
    /// </summary>
    public async Task<HealthResponse> Execute()
    {
        return await Execute<HealthResponse>(BuildUri());
    }
}