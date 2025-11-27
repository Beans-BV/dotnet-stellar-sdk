using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents AccountMerge operation response.
/// </summary>
public class AccountMergeOperationResponse : OperationResponse
{
    public override int TypeId => 8;

    [JsonPropertyName("account")]
    public string Account { get; init; }

    [JsonPropertyName("account_muxed")]
    public string AccountMuxed { get; init; }

    [JsonPropertyName("account_muxed_id")]
    public ulong? AccountMuxedID { get; init; }

    [JsonPropertyName("into")] public string Into { get; init; }

    [JsonPropertyName("into_muxed")]
    public string IntoMuxed { get; init; }

    [JsonPropertyName("into_muxed_id")]
    public ulong? IntoMuxedID { get; init; }
}