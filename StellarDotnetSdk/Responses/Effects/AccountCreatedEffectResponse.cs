using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the account_created effect response.
///     This effect occurs when a new account is created on the network.
/// </summary>
public sealed class AccountCreatedEffectResponse : EffectResponse
{
    /// <summary>
    ///     The starting balance of the newly created account in XLM.
    /// </summary>
    [JsonPropertyName("starting_balance")]
    public string? StartingBalance { get; init; }

    /// <inheritdoc />
    public override int TypeId => 0;
}