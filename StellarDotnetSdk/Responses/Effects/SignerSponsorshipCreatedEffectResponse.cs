using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents signer_sponsorship_created effect response.
/// </summary>
public class SignerSponsorshipCreatedEffectResponse : EffectResponse
{
    public override int TypeId => 72;

    [JsonProperty(PropertyName = "signer")]
    public string Signer { get; init; }

    [JsonProperty(PropertyName = "sponsor")]
    public string Sponsor { get; init; }
}