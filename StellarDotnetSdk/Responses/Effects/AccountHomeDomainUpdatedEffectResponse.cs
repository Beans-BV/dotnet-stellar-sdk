using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the account_home_domain_updated effect response.
///     This effect occurs when an account's home domain is changed.
/// </summary>
public sealed class AccountHomeDomainUpdatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 5;

    /// <summary>
    ///     The new home domain set on the account.
    ///     The home domain is where the stellar.toml file can be found.
    /// </summary>
    [JsonPropertyName("home_domain")]
    public required string HomeDomain { get; init; }
}