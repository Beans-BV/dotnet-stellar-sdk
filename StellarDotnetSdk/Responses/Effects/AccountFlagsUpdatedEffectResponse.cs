using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the account_flags_updated effect response.
///     This effect occurs when an account's authorization flags are changed.
/// </summary>
public sealed class AccountFlagsUpdatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 6;

    /// <summary>
    ///     Whether the AUTH_REQUIRED flag is set on the account.
    ///     When true, anyone wanting to hold an asset issued by this account must be authorized.
    /// </summary>
    [JsonPropertyName("auth_required_flag")]
    public bool AuthRequiredFlag { get; init; }

    /// <summary>
    ///     Whether the AUTH_REVOCABLE flag is set on the account.
    ///     When true, the issuer can revoke authorization for assets they've issued.
    /// </summary>
    [JsonPropertyName("auth_revocable_flag")]
    public bool AuthRevocableFlag { get; init; }
}