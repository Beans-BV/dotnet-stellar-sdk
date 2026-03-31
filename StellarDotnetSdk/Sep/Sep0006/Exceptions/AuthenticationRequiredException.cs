namespace StellarDotnetSdk.Sep.Sep0006.Exceptions;

/// <summary>
///     Exception thrown when an endpoint requires SEP-10 authentication
///     but no valid JWT token was provided or the provided token was invalid or expired.
/// </summary>
public class AuthenticationRequiredException : TransferServerException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthenticationRequiredException" /> class.
    /// </summary>
    public AuthenticationRequiredException()
        : base("The endpoint requires authentication.")
    {
    }
}