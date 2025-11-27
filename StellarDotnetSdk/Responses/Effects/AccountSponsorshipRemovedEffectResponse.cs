using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the account_sponsorship_removed effect response.
///     This effect occurs when an account's sponsorship is removed.
/// </summary>
public sealed class AccountSponsorshipRemovedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 62;

    /// <summary>
    ///     The account ID of the former sponsor.
    /// </summary>
    [JsonPropertyName("former_sponsor")]
    public string? FormerSponsor { get; init; }
}