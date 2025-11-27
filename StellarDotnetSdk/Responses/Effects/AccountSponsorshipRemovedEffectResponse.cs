using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents account_sponsorship_removed effect response.
/// </summary>
public class AccountSponsorshipRemovedEffectResponse : EffectResponse
{
    public override int TypeId => 62;

    [JsonPropertyName("former_sponsor")]
    public string FormerSponsor { get; init; }
}