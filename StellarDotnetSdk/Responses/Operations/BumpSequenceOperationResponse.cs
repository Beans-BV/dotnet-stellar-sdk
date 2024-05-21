using System.Text.Json;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents AccountMerge operation response.
/// </summary>
public class BumpSequenceOperationResponse : OperationResponse
{
    public override int TypeId => 11;

    [JsonPropertyName("bump_to")]
    public long BumpTo { get; init; }
}