namespace StellarDotnetSdk.Sep.Sep0007.Exceptions;

/// <summary>
///     Thrown when a URI carries an <c>origin_domain</c> but no <c>signature</c>. SEP-7 treats such a request
///     as invalid: the wallet must not display the domain and the user should not sign it.
/// </summary>
public class MissingSignatureException : Sep7Exception
{
    /// <summary>Initializes a new instance of the <see cref="MissingSignatureException" /> class.</summary>
    public MissingSignatureException()
        : base("The URI has an 'origin_domain' but no 'signature' parameter.") { }
}
