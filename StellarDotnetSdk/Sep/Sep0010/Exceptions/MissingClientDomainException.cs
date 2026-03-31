namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Exception thrown when clientDomainSigningDelegate is provided without clientDomain.
/// </summary>
public class MissingClientDomainException : WebAuthException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MissingClientDomainException" /> class.
    /// </summary>
    public MissingClientDomainException()
        : base("The clientDomain is required if clientDomainSigningDelegate is provided")
    {
    }
}