using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents AccountMerge operation response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/operation.html
///     <seealso cref="Requests.OperationsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
[JsonObject]
public class AccountMergeOperationResponse : OperationResponse
{
    public override int TypeId => 8;

    [JsonProperty(PropertyName = "account")]
    public string Account { get; private set; }

    [JsonProperty(PropertyName = "account_muxed")]
    public string AccountMuxed { get; private set; }

    [JsonProperty(PropertyName = "account_muxed_id")]
    public ulong? AccountMuxedID { get; private set; }

    [JsonProperty(PropertyName = "into")] public string Into { get; private set; }

    [JsonProperty(PropertyName = "into_muxed")]
    public string IntoMuxed { get; private set; }

    [JsonProperty(PropertyName = "into_muxed_id")]
    public ulong? IntoMuxedID { get; private set; }
}