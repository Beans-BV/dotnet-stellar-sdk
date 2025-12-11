using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the trustline_sponsorship_created effect response.
///     This effect occurs when a trustline becomes sponsored.
/// </summary>
public sealed class TrustlineSponsorshipCreatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 63;

    /// <summary>
    ///     The sponsored asset (in canonical form: "code:issuer" or "native").
    /// </summary>
    [JsonPropertyName("asset")]
    public required string Asset { get; init; }

    /// <summary>
    ///     The account ID of the sponsor.
    /// </summary>
    [JsonPropertyName("sponsor")]
    public required string Sponsor { get; init; }
}