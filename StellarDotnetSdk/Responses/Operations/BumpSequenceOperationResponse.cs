using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents AccountMerge operation response.
/// </summary>
public class BumpSequenceOperationResponse : OperationResponse
{
    public override int TypeId => 11;

    [JsonProperty(PropertyName = "bump_to")]
    public long BumpTo { get; init; }
}