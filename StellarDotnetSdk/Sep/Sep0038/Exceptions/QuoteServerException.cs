using System;

namespace StellarDotnetSdk.Sep.Sep0038.Exceptions;

/// <summary>
///     Base type of every error <see cref="QuoteService" /> raises for a response from, or the discovery of, an
///     anchor's SEP-38 quote server. Catch this to handle all of them.
/// </summary>
/// <remarks>
///     Transport failures (DNS, TLS, connection resets, timeouts) are not wrapped: they surface as the
///     <see cref="System.Net.Http.HttpRequestException" /> or <see cref="System.Threading.Tasks.TaskCanceledException" />
///     that <see cref="System.Net.Http.HttpClient" /> raised. Invalid request arguments raise
///     <see cref="ArgumentException" /> before anything is sent.
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
    public QuoteServerException(string message, int? statusCode, string? error, string? responseBody,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Error = error;
        ResponseBody = responseBody;
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
}
