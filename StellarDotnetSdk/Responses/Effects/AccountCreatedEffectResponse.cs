using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable
/// <summary>
///     Represents account_created effect response.
/// </summary>
public class AccountCreatedEffectResponse : EffectResponse
{
    [JsonProperty(PropertyName = "starting_balance")]
    public string StartingBalance { get; init; }

    public override int TypeId => 0;
}