using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents account_sponsorship_created effect response.
/// </summary>
public class AccountSponsorshipCreatedEffectResponse : EffectResponse
{
    public override int TypeId => 60;

    [JsonProperty(PropertyName = "sponsor")]
    public string Sponsor { get; init; }
}