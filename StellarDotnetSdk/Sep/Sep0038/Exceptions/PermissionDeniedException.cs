namespace StellarDotnetSdk.Sep.Sep0038.Exceptions;

/// <summary>
///     The anchor's SEP-38 quote server answered <c>403 Permission Denied</c>: no <c>Authorization</c> header was
///     provided or its JWT was not accepted, or the anchor denied access (for example to an account that is not
///     KYC'ed).
/// </summary>
/// <remarks>
///     <see cref="QuoteServerException.Error" /> holds the anchor's explanation, when it sent one.
/// </remarks>
public class PermissionDeniedException : QuoteServerException
{
    /// <summary>
    ///     Initializes a new instance.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="error">The <c>error</c> field of the response body, if it had one.</param>
    /// <param name="responseBody">The response body.</param>
    public PermissionDeniedException(string message, string? error, string? responseBody)
        : base(message, 403, error, responseBody)
    {
    }
}
