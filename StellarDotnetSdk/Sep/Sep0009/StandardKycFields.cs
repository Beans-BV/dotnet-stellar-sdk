namespace StellarDotnetSdk.Sep.Sep0009;

/// <summary>
///     Implements SEP-0009 - Standard KYC Fields for Stellar Ecosystem.
///     Defines standardized Know Your Customer (KYC) and Anti-Money Laundering (AML)
///     fields for use across the Stellar ecosystem. Anchors, exchanges, and other
///     regulated entities should use these fields for consistent identity verification.
///     See <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0009.md">SEP-0009</a>
/// </summary>
public sealed record StandardKycFields
{
    /// <summary>
    ///     KYC fields for natural persons (individuals).
    /// </summary>
    public NaturalPersonKycFields? NaturalPerson { get; init; }

    /// <summary>
    ///     KYC fields for organizations (businesses).
    /// </summary>
    public OrganizationKycFields? Organization { get; init; }
}

