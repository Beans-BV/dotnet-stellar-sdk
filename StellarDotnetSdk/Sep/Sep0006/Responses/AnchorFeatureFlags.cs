using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Feature flags indicating anchor capabilities for SEP-6 operations.
/// </summary>
public sealed class AnchorFeatureFlags : Response
{
    /// <summary>
    ///     Whether or not the anchor supports creating accounts for users requesting
    ///     deposits. Defaults to true.
    /// </summary>
    [JsonPropertyName("account_creation")]
    public bool AccountCreation { get; init; } = true;

    /// <summary>
    ///     Whether or not the anchor supports sending deposit funds as claimable
    ///     balances. This is relevant for users of Stellar accounts without a
    ///     trustline to the requested asset. Defaults to false.
    /// </summary>
    [JsonPropertyName("claimable_balances")]
    public bool ClaimableBalances { get; init; }
}