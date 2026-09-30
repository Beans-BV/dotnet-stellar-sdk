using System;

namespace StellarDotnetSdk.Sep.Sep0038.Exceptions;

/// <summary>
///     The anchor's SEP-38 quote server sent a response the client cannot use: a status code other than
///     <c>400</c>, <c>403</c> or <c>404</c> outside the 2xx range, a body larger than the size limit, or a success
///     body that is not a valid response — malformed JSON, a duplicated property, a missing required field, or an
///     amount that cannot be represented exactly.
/// </summary>
/// <remarks>
///     When a success body failed to deserialize, <see cref="Exception.InnerException" /> is the
///     <see cref="System.Text.Json.JsonException" /> that describes why.
/// </remarks>
public class UnexpectedResponseException : QuoteServerException
{
    /// <summary>
    ///     Initializes a new instance.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="error">The <c>error</c> field of the response body, if it had one.</param>
    /// <param name="responseBody">The response body, if it was read.</param>
    /// <param name="innerException">The exception that caused this one, if any.</param>
    public UnexpectedResponseException(string message, int statusCode, string? error, string? responseBody,
        Exception? innerException = null)
        : base(message, statusCode, error, responseBody, innerException)
    {
    }
}
