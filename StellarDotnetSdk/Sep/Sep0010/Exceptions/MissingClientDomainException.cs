namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Exception thrown when clientDomainSigningDelegate is provided without clientDomain.
/// </summary>
public class MissingClientDomainException : WebAuthException
{
    public MissingClientDomainException()
        : base("The clientDomain is required if clientDomainSigningDelegate is provided")
    {
    }
}