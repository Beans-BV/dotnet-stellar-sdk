using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0024.Responses;

/// <summary>
///     Feature flags indicating optional capabilities supported by the anchor.
///     These flags help clients understand what advanced features the anchor supports for deposits and withdrawals.
/// </summary>
public class FeatureFlags : Response
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FeatureFlags" /> class.
    /// </summary>
    /// <param name="accountCreation">Whether the anchor supports creating accounts for users requesting deposits. Defaults to true.</param>
    /// <param name="claimableBalances">Whether the anchor supports sending deposit funds as claimable balances. Defaults to false.</param>
    [JsonConstructor]
    public FeatureFlags(bool accountCreation = true, bool claimableBalances = false)
    {
        AccountCreation = accountCreation;
        ClaimableBalances = claimableBalances;
    }

    /// <summary>
    ///     Gets a value indicating whether the anchor supports creating accounts for users requesting deposits.
    ///     When true, the anchor will create a Stellar account if one doesn't exist.
    ///     Defaults to true.
    /// </summary>
    [JsonPropertyName("account_creation")]
    public bool AccountCreation { get; }

    /// <summary>
    ///     Gets a value indicating whether the anchor supports sending deposit funds as claimable balances.
    ///     This is relevant for users without a trustline to the requested asset.
    ///     Defaults to false.
    /// </summary>
    [JsonPropertyName("claimable_balances")]
    public bool ClaimableBalances { get; }
}

