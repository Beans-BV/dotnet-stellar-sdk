using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents account_created effect response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class AccountCreatedEffectResponse : EffectResponse
{
    public AccountCreatedEffectResponse()
    {
    }

    /// <inheritdoc />
    public AccountCreatedEffectResponse(string startingBalance)
    {
        StartingBalance = startingBalance;
    }

    [JsonProperty(PropertyName = "starting_balance")]
    public string StartingBalance { get; private set; }

    public override int TypeId => 0;
}