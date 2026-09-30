namespace StellarDotnetSdk.Sep.Sep0007.Exceptions;

/// <summary>
///     Thrown when a request signature is created or verified for a URI that has no <c>origin_domain</c>
///     parameter. Without it there is no stellar.toml to take the <c>URI_REQUEST_SIGNING_KEY</c> from.
/// </summary>
public class MissingOriginDomainException : Sep7Exception
{
    /// <summary>Initializes a new instance of the <see cref="MissingOriginDomainException" /> class.</summary>
    public MissingOriginDomainException()
        : base("The URI has no 'origin_domain' parameter.") { }
}
