using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Abstract base class for all Horizon API responses.
///     Provides common functionality for handling rate limit headers.
/// </summary>
public abstract class Response
{
    private const string XRateLimitLimit = "X-Ratelimit-Limit";
    private const string XRateLimitRemaining = "X-Ratelimit-Remaining";
    private const string XRateLimitReset = "X-Ratelimit-Reset";

    /// <summary>
    ///     The maximum number of requests allowed per hour for this endpoint.
    /// </summary>
    [JsonIgnore]
    protected int RateLimitLimit { get; private set; }

    /// <summary>
    ///     The number of requests remaining in the current rate limit window.
    /// </summary>
    [JsonIgnore]
    protected int RateLimitRemaining { get; private set; }

    /// <summary>
    ///     The Unix timestamp (in seconds) when the rate limit window resets.
    /// </summary>
    [JsonIgnore]
    protected int RateLimitReset { get; private set; }

    /// <summary>
    ///     The client that fetched this response, or <c>null</c> when none was recorded: the caller deserialized
    ///     it, or a path other than <see cref="Requests.RequestBuilder{T}.Execute{TZ}" /> and
    ///     <see cref="Link{TResponse}" />'s <c>Follow</c> fetched it. <see cref="Page{T}" /> follows its links with
    ///     this client.
    /// </summary>
    [JsonIgnore]
    internal HttpClient? HttpClient { get; set; }

    /// <summary>
    ///     The absolute URI of the request that fetched this response (a relative request URI resolved against
    ///     the client's base address), recorded together with <see cref="HttpClient" />. <see cref="Page{T}" />
    ///     sends its follows to this URI's scheme, host and port.
    /// </summary>
    [JsonIgnore]
    internal Uri? RequestUri { get; set; }

    /// <summary>
    ///     Sets the rate limit information from HTTP response headers.
    /// </summary>
    /// <param name="headers">The HTTP response headers containing rate limit information.</param>
    public void SetHeaders(HttpResponseHeaders headers)
    {
        if (headers.Contains(XRateLimitLimit))
        {
            RateLimitLimit = int.Parse(headers.GetValues(XRateLimitLimit).First());
        }

        if (headers.Contains(XRateLimitRemaining))
        {
            RateLimitRemaining = int.Parse(headers.GetValues(XRateLimitRemaining).First());
        }

        if (headers.Contains(XRateLimitReset))
        {
            RateLimitReset = int.Parse(headers.GetValues(XRateLimitReset).First());
        }
    }
}