using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the signer_sponsorship_updated effect response.
///     This effect occurs when a signer's sponsor changes.
/// </summary>
public sealed class SignerSponsorshipUpdatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 73;

    /// <summary>
    ///     The public key of the signer.
    /// </summary>
    [JsonPropertyName("signer")]
    public required string Signer { get; init; }

    /// <summary>
    ///     The account ID of the former sponsor.
    /// </summary>
    [JsonPropertyName("former_sponsor")]
    public required string FormerSponsor { get; init; }

    /// <summary>
    ///     The account ID of the new sponsor.
    /// </summary>
    [JsonPropertyName("new_sponsor")]
    public required string NewSponsor { get; init; }
}