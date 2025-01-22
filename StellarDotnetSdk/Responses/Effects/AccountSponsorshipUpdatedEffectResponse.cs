using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable
/// <summary>
///     Represents account_sponsorship_updated effect response.
/// </summary>
public class AccountSponsorshipUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 61;


    [JsonPropertyName("former_sponsor")]
    public string FormerSponsor { get; init; }

    [JsonPropertyName("new_sponsor")]
    public string NewSponsor { get; init; }
}