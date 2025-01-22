using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents account_sponsorship_created effect response.
/// </summary>
public class AccountSponsorshipCreatedEffectResponse : EffectResponse
{
    public override int TypeId => 60;

    [JsonPropertyName("sponsor")]
    public string Sponsor { get; init; }
}