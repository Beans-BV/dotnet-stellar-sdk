using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Sep.Sep0045;

/// <summary>
///     Result of parsing and validating a SEP-45 challenge. Contains the decoded Soroban
///     authorization entries plus every field extracted from the <c>web_auth_verify</c> args map.
/// </summary>
/// <param name="Entries">All decoded SorobanAuthorizationEntries in the challenge.</param>
/// <param name="ClientAccountId">The client contract account ID being authenticated (C... address).</param>
/// <param name="HomeDomain">The home domain the challenge was issued for.</param>
/// <param name="WebAuthDomain">The web auth domain binding the challenge.</param>
/// <param name="WebAuthDomainAccountId">The server account that signed the web auth domain commitment (G... address).</param>
/// <param name="Nonce">The server-generated nonce string embedded in the challenge.</param>
/// <param name="ClientDomain">Optional client domain the challenge was requested from.</param>
/// <param name="ClientDomainAccountId">Optional client domain's signing account (G... address); paired with <paramref name="ClientDomain" />.</param>
public sealed record ChallengeAuthorizationEntries(
    SorobanAuthorizationEntry[] Entries,
    string ClientAccountId,
    string HomeDomain,
    string WebAuthDomain,
    string WebAuthDomainAccountId,
    string Nonce,
    string? ClientDomain,
    string? ClientDomainAccountId);
