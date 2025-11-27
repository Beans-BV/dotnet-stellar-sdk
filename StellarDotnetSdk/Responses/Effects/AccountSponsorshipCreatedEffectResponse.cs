using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the account_sponsorship_created effect response.
///     This effect occurs when an account becomes sponsored.
/// </summary>
public sealed class AccountSponsorshipCreatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 60;

    /// <summary>
    ///     The account ID of the sponsor.
    /// </summary>
    [JsonPropertyName("sponsor")]
    public string? Sponsor { get; init; }
}