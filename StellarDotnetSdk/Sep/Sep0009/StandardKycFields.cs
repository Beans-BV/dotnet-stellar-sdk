using System.Collections.Generic;

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

    /// <summary>
    ///     Converts all card KYC fields to a map for SEP-9 submission.
    /// </summary>
    /// <returns>Dictionary containing all non-null field values</returns>
    public IReadOnlyDictionary<string, string> GetFields()
    {
        var result = new Dictionary<string, string>();
        if (NaturalPerson is not null)
        {
            foreach (var kvp in NaturalPerson.GetFields())
            {
                result[kvp.Key] = kvp.Value;
            }
        }
        if (Organization is not null)
        {
            foreach (var kvp in Organization.GetFields())
            {
                result[kvp.Key] = kvp.Value;
            }
        }
        return result;
    }
}