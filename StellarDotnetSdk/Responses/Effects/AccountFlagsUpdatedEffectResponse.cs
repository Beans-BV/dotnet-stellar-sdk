using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents account_flags_updated effect response.
/// </summary>
public class AccountFlagsUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 6;

    [JsonPropertyName("auth_required_flag")]
    public bool AuthRequiredFlag { get; init; }

    [JsonPropertyName("auth_revocable_flag")]
    public bool AuthRevocableFlag { get; init; }
}