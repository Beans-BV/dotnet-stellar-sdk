namespace StellarDotnetSdk.Sep.Sep0038.Exceptions;

/// <summary>
///     The anchor's SEP-38 quote server answered <c>400 Bad Request</c>: the request was invalid in some way, for
///     example an unsupported asset or a missing required parameter.
/// </summary>
/// <remarks>
///     <see cref="QuoteServerException.Error" /> holds the anchor's explanation, when it sent one.
/// </remarks>
public class BadRequestException : QuoteServerException
{
    /// <summary>
    ///     Initializes a new instance.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="error">The <c>error</c> field of the response body, if it had one.</param>
    /// <param name="responseBody">The response body.</param>
    public BadRequestException(string message, string? error, string? responseBody)
        : base(message, 400, error, responseBody)
    {
    }
}
