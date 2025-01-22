using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents AccountMerge operation response.
/// </summary>
public class BumpSequenceOperationResponse : OperationResponse
{
    public override int TypeId => 11;

    [JsonPropertyName("bump_to")]
    public string BumpTo { get; init; }
}