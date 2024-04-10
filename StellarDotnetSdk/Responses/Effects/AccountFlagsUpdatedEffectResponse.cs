using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents account_flags_updated effect response.
/// </summary>
public class AccountFlagsUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 6;

    [JsonProperty(PropertyName = "auth_required_flag")]
    public bool AuthRequiredFlag { get; init; }

    [JsonProperty(PropertyName = "auth_revocable_flag")]
    public bool AuthRevocableFlag { get; init; }
}