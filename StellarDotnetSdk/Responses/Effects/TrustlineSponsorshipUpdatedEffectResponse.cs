using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the trustline_sponsorship_updated effect response.
///     This effect occurs when a trustline's sponsor changes.
/// </summary>
public sealed class TrustlineSponsorshipUpdatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 64;

    /// <summary>
    ///     The sponsored asset (in canonical form: "code:issuer" or "native").
    /// </summary>
    [JsonPropertyName("asset")]
    public string? Asset { get; init; }

    /// <summary>
    ///     The account ID of the former sponsor.
    /// </summary>
    [JsonPropertyName("former_sponsor")]
    public string? FormerSponsor { get; init; }

    /// <summary>
    ///     The account ID of the new sponsor.
    /// </summary>
    [JsonPropertyName("new_sponsor")]
    public string? NewSponsor { get; init; }
}