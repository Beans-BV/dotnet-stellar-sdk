using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable
/// <summary>
///     Represents account_created effect response.
/// </summary>
public class AccountCreatedEffectResponse : EffectResponse
{
    [JsonPropertyName("starting_balance")]
    public string StartingBalance { get; init; }

    public override int TypeId => 0;
}