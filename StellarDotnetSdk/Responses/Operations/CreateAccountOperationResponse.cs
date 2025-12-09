using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a create_account operation response.
///     Creates and funds a new account with a starting balance of XLM.
/// </summary>
public class CreateAccountOperationResponse : OperationResponse
{
    public override int TypeId => 0;

    /// <summary>
    ///     The address of the new account that was created.
    /// </summary>
    [JsonPropertyName("account")]
    public required string Account { get; init; }

    /// <summary>
    ///     The account that funded the new account.
    /// </summary>
    [JsonPropertyName("funder")]
    public required string Funder { get; init; }

    /// <summary>
    ///     The muxed account representation of the funder, if applicable.
    /// </summary>
    [JsonPropertyName("funder_muxed")]
    public string? FunderMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the funder, if applicable.
    /// </summary>
    [JsonPropertyName("funder_muxed_id")]
    public ulong? FunderMuxedId { get; init; }

    /// <summary>
    ///     The amount of XLM sent to the new account.
    /// </summary>
    [JsonPropertyName("starting_balance")]
    public required string StartingBalance { get; init; }
}