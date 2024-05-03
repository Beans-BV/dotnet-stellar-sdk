using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents account_sponsorship_removed effect response.
/// </summary>
public class AccountSponsorshipRemovedEffectResponse : EffectResponse
{
    public override int TypeId => 62;

    [JsonProperty(PropertyName = "former_sponsor")]
    public string FormerSponsor { get; init; }
}