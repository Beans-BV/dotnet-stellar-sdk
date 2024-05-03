using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents CreateAccount operation response.
/// </summary>
public class CreateAccountOperationResponse : OperationResponse
{
    public override int TypeId => 0;

    [JsonProperty(PropertyName = "account")]
    public string Account { get; init; }

    [JsonProperty(PropertyName = "funder")]
    public string Funder { get; init; }

    [JsonProperty(PropertyName = "funder_muxed")]
    public string FunderMuxed { get; init; }

    [JsonProperty(PropertyName = "funder_muxed_id")]
    public ulong? FunderMuxedId { get; init; }

    [JsonProperty(PropertyName = "starting_balance")]
    public string StartingBalance { get; init; }
}