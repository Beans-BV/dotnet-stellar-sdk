using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents CreateAccount operation response.
/// </summary>

public class CreateAccountOperationResponse : OperationResponse
{
    public override int TypeId => 0;

    [JsonPropertyName("account")]
    public string Account { get; init; }

    [JsonPropertyName("funder")]
    public string Funder { get; init; }

    [JsonPropertyName("funder_muxed")]
    public string FunderMuxed { get; init; }

    [JsonPropertyName("funder_muxed_id")]
    public ulong? FunderMuxedId { get; init; }

    [JsonPropertyName("starting_balance")]
    public string StartingBalance { get; init; }
}