using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the signer_sponsorship_created effect response.
///     This effect occurs when a signer becomes sponsored.
/// </summary>
public sealed class SignerSponsorshipCreatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 72;

    /// <summary>
    ///     The public key of the sponsored signer.
    /// </summary>
    [JsonPropertyName("signer")]
    public string? Signer { get; init; }

    /// <summary>
    ///     The account ID of the sponsor.
    /// </summary>
    [JsonPropertyName("sponsor")]
    public string? Sponsor { get; init; }
}