namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when a clientDomainSigningDelegate is supplied without a corresponding clientDomain.</summary>
public class MissingClientDomainException : WebAuthContractException
{
    /// <summary>Initializes a new instance of the <see cref="MissingClientDomainException" /> class.</summary>
    public MissingClientDomainException()
        : base("A clientDomain must be supplied when using clientDomainSigningDelegate.")
    {
    }
}
