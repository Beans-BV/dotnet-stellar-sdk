using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents AccountMerge operation response.
/// </summary>
[JsonObject]
public class AccountMergeOperationResponse : OperationResponse
{
    public override int TypeId => 8;

    [JsonProperty(PropertyName = "account")]
    public string Account { get; init; }

    [JsonProperty(PropertyName = "account_muxed")]
    public string AccountMuxed { get; init; }

    [JsonProperty(PropertyName = "account_muxed_id")]
    public ulong? AccountMuxedID { get; init; }

    [JsonProperty(PropertyName = "into")] public string Into { get; init; }

    [JsonProperty(PropertyName = "into_muxed")]
    public string IntoMuxed { get; init; }

    [JsonProperty(PropertyName = "into_muxed_id")]
    public ulong? IntoMuxedID { get; init; }
}