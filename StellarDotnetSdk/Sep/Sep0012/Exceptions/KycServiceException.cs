using System;

namespace StellarDotnetSdk.Sep.Sep0012.Exceptions;

/// <summary>
///     Base exception for SEP-0012 KYC API errors. Thrown as-is for any non-success HTTP status that has no more
///     specific subclass (typically <c>400 Bad Request</c>, <c>429</c> or a <c>5xx</c>), for a response that arrived
///     from a different origin after a redirect (its <see cref="StatusCode" /> is that response's status, possibly
///     a success), and when the anchor's KYC server cannot be discovered from its stellar.toml.
/// </summary>
/// <remarks>
///     SEP-0012 error responses carry a human-readable description under the <c>error</c> key; it is exposed through
///     <see cref="ErrorMessage" /> verbatim. <see cref="Exception.Message" /> quotes a clamped copy of it in which
///     control, format, line-separator and paragraph-separator characters are replaced with spaces, because the text is
///     server-supplied and exception messages routinely end up in logs.
/// </remarks>
public class KycServiceException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="KycServiceException" /> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="statusCode">The HTTP status code of the response, or <c>null</c> when no response was involved.</param>
    /// <param name="errorMessage">The value of the <c>error</c> key in the response body, if any.</param>
    /// <param name="innerException">The exception that caused this one, if any.</param>
    /// <param name="retryAfterDelay">The delay the server asked for in a <c>Retry-After</c> header, if any.</param>
    public KycServiceException(
        string message,
        int? statusCode = null,
        string? errorMessage = null,
        Exception? innerException = null,
        TimeSpan? retryAfterDelay = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
        RetryAfterDelay = retryAfterDelay;
    }

    /// <summary>
    ///     Gets the HTTP status code returned by the KYC server, or <c>null</c> when the error did not come from an
    ///     HTTP response (for example, when stellar.toml declares no KYC server).
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    ///     Gets the anchor-supplied description from the <c>error</c> key of the response body, or <c>null</c> when
    ///     the body carried none or could not be parsed.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    ///     Gets the delay the server asked the client to wait before retrying, from the <c>Retry-After</c> header of a
    ///     response such as <c>429 Too Many Requests</c> or <c>503 Service Unavailable</c>, or <c>null</c> when the
    ///     response carried none. Set only on a plain <see cref="KycServiceException" />; the more specific subclasses
    ///     are not raised for those statuses. Named like <c>TooManyRequestsException.RetryAfterDelay</c>; there,
    ///     <c>RetryAfter</c> is the same value in whole seconds.
    /// </summary>
    public TimeSpan? RetryAfterDelay { get; }
}
