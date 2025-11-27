using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the trustline_sponsorship_removed effect response.
///     This effect occurs when a trustline's sponsorship is removed.
/// </summary>
public sealed class TrustlineSponsorshipRemovedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 65;

    /// <summary>
    ///     The asset that was sponsored (in canonical form: "code:issuer" or "native").
    /// </summary>
    [JsonPropertyName("asset")]
    public string? Asset { get; init; }

    /// <summary>
    ///     The account ID of the former sponsor.
    /// </summary>
    [JsonPropertyName("former_sponsor")]
    public string? FormerSponsor { get; init; }
}