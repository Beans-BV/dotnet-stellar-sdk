using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents account_home_domain_updated effect response.
/// </summary>
public class AccountHomeDomainUpdatedEffectResponse : EffectResponse
{
    [JsonProperty(PropertyName = "home_domain")]
    public string HomeDomain { get; init; }

    public override int TypeId => 5;
}