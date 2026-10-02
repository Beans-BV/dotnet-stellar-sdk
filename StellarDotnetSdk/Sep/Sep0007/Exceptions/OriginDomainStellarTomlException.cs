using System;

namespace StellarDotnetSdk.Sep.Sep0007.Exceptions;

/// <summary>
///     Thrown when the stellar.toml of the <c>origin_domain</c> cannot be fetched or parsed. SEP-7 treats the
///     request as invalid in that case.
/// </summary>
public class OriginDomainStellarTomlException : Sep7Exception
{
    /// <summary>Initializes a new instance of the <see cref="OriginDomainStellarTomlException" /> class.</summary>
    /// <param name="domain">The origin domain whose stellar.toml could not be loaded.</param>
    /// <param name="message">The reason the stellar.toml could not be loaded.</param>
    public OriginDomainStellarTomlException(string domain, string message)
        : base($"Could not load stellar.toml for origin domain '{domain}': {message}")
    {
        Domain = domain;
    }

    /// <summary>Initializes a new instance of the <see cref="OriginDomainStellarTomlException" /> class.</summary>
    /// <param name="domain">The origin domain whose stellar.toml could not be loaded.</param>
    /// <param name="message">The reason the stellar.toml could not be loaded.</param>
    /// <param name="innerException">The underlying cause of the failure.</param>
    public OriginDomainStellarTomlException(string domain, string message, Exception innerException)
        : base($"Could not load stellar.toml for origin domain '{domain}': {message}", innerException)
    {
        Domain = domain;
    }

    /// <summary>The origin domain whose stellar.toml could not be loaded.</summary>
    public string Domain { get; }
}
