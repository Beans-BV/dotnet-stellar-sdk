using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents account_home_domain_updated effect response.
/// </summary>
public class AccountHomeDomainUpdatedEffectResponse : EffectResponse
{
    [JsonPropertyName("home_domain")]
    public string HomeDomain { get; init; }

    public override int TypeId => 5;
}