using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents signer_sponsorship_created effect response.
/// </summary>
public class SignerSponsorshipCreatedEffectResponse : EffectResponse
{
    public override int TypeId => 72;

    [JsonPropertyName("signer")]
    public string Signer { get; init; }

    [JsonPropertyName("sponsor")]
    public string Sponsor { get; init; }
}