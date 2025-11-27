using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the signer_sponsorship_removed effect response.
///     This effect occurs when a signer's sponsorship is removed.
/// </summary>
public sealed class SignerSponsorshipRemovedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 74;

    /// <summary>
    ///     The public key of the signer.
    /// </summary>
    [JsonPropertyName("signer")]
    public string? Signer { get; init; }

    /// <summary>
    ///     The account ID of the former sponsor.
    /// </summary>
    [JsonPropertyName("former_sponsor")]
    public string? FormerSponsor { get; init; }
}