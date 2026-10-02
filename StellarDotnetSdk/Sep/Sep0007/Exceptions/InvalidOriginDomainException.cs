namespace StellarDotnetSdk.Sep.Sep0007.Exceptions;

/// <summary>Thrown when the <c>origin_domain</c> parameter is not a fully qualified domain name.</summary>
public class InvalidOriginDomainException : InvalidSep7UriException
{
    /// <summary>Initializes a new instance of the <see cref="InvalidOriginDomainException" /> class.</summary>
    /// <param name="originDomain">The rejected <c>origin_domain</c> value.</param>
    public InvalidOriginDomainException(string originDomain)
        : base($"The 'origin_domain' parameter {Sep7UriParser.Echo(originDomain)} is not a fully qualified domain name.")
    {
        OriginDomain = originDomain;
    }

    /// <summary>The rejected <c>origin_domain</c> value.</summary>
    public string OriginDomain { get; }
}
