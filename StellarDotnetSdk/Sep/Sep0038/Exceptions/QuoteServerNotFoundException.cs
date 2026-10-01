namespace StellarDotnetSdk.Sep.Sep0038.Exceptions;

/// <summary>
///     The anchor's SEP-38 quote server answered <c>404 Not Found</c>: the resource does not exist. For <c>GET
///     /quote/:id</c>, no quote with that id is visible to the authenticated account; some anchors (the Anchor
///     Platform among them) also answer 404 to <c>GET /price</c> or <c>GET /prices</c> for an asset they do not
///     list, which the specification does not define.
/// </summary>
/// <remarks>
///     <see cref="QuoteServerException.Error" /> holds the anchor's explanation, when it sent one.
/// </remarks>
public class QuoteServerNotFoundException : QuoteServerException
{
    /// <summary>
    ///     Initializes a new instance.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="error">The <c>error</c> field of the response body, if it had one.</param>
    /// <param name="responseBody">The response body.</param>
    public QuoteServerNotFoundException(string message, string? error, string? responseBody)
        : base(message, 404, error, responseBody)
    {
    }
}
