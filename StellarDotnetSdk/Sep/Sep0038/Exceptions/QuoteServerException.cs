using System;

namespace StellarDotnetSdk.Sep.Sep0038.Exceptions;

/// <summary>
///     Base type of the errors <see cref="QuoteService" /> raises for a response from an anchor's SEP-38 quote
///     server, or for a stellar.toml that declares no usable one. Catch this to handle all of them.
/// </summary>
/// <remarks>
///     A stellar.toml that cannot be fetched or parsed during discovery raises
///     <see cref="StellarDotnetSdk.Sep.Sep0001.Exceptions.StellarTomlException" />, which does not derive from this type,
///     and a domain that is empty or is not a valid host name raises <see cref="ArgumentException" /> or
///     <see cref="UriFormatException" />. Transport failures (DNS, TLS, connection resets, timeouts) are not wrapped:
///     they surface as the <see cref="System.Net.Http.HttpRequestException" /> or
///     <see cref="System.Threading.Tasks.TaskCanceledException" /> that <see cref="System.Net.Http.HttpClient" />
///     raised. Invalid request arguments, a malformed JWT among them, raise <see cref="ArgumentException" /> before
///     anything is sent; on runtimes before .NET 10, a request URL beyond the runtime's length limit (a quote id tens
///     of kilobytes long) raises <see cref="UriFormatException" />.
/// </remarks>
public class QuoteServerException : Exception
{
    /// <summary>
    ///     Initializes a new instance with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public QuoteServerException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance with a message and the exception that caused it.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public QuoteServerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance describing an HTTP response.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="error">The <c>error</c> field of the response body, if it had one.</param>
    /// <param name="responseBody">The response body, if it was read.</param>
    /// <param name="innerException">The exception that caused this one, if any.</param>
    /// <param name="retryAfterDelay">The delay the server asked for in a <c>Retry-After</c> header, if any.</param>
    public QuoteServerException(string message, int? statusCode, string? error, string? responseBody,
        Exception? innerException = null, TimeSpan? retryAfterDelay = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Error = error;
        ResponseBody = responseBody;
        RetryAfterDelay = retryAfterDelay;
    }

    /// <summary>
    ///     The HTTP status code of the response, or <see langword="null" /> when the error did not come from a
    ///     response.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    ///     The human-readable <c>error</c> field of the anchor's error body, exactly as sent, or
    ///     <see langword="null" /> when the body had none. Server-controlled: escape it before logging or display.
    ///     The exception <see cref="Exception.Message" /> carries an escaped, length-bounded copy.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    ///     The raw response body (at most the size limit <see cref="QuoteService" /> reads), or
    ///     <see langword="null" /> when it was not read. Server-controlled: escape it before logging or display.
    /// </summary>
    public string? ResponseBody { get; }

    /// <summary>
    ///     The delay the server asked the client to wait before retrying, from the <c>Retry-After</c> header of an
    ///     error response such as <c>429 Too Many Requests</c> or <c>503 Service Unavailable</c>, or
    ///     <see langword="null" /> when the response carried none. Only an <see cref="UnexpectedResponseException" />
    ///     for an error status carries it, whatever the size of the error body; the 400, 403 and 404 subtypes never do. Named like
    ///     <see cref="StellarDotnetSdk.Exceptions.TooManyRequestsException.RetryAfterDelay" />.
    /// </summary>
    /// <remarks>
    ///     <c>POST /quote</c> must not be retried automatically: each call reserves a new firm quote. The default
    ///     resilience options retry nothing, but the <c>ForHorizon()</c> and <c>ForSoroban()</c> presets retry
    ///     <c>POST</c> on a 408, 429, 500, 502, 503 or 504 answer, so configure the service with options whose
    ///     <see cref="StellarDotnetSdk.Requests.HttpResilienceOptions.RetryHttpMethods" /> leaves out <c>POST</c>
    ///     (connection-failure retries still replay every method; see the README). A 429 then surfaces here, and
    ///     this delay is how the caller honors the anchor's rate limit before asking for another quote.
    /// </remarks>
    public TimeSpan? RetryAfterDelay { get; }
}
