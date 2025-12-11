using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a bump_sequence operation response.
///     Bumps forward the sequence number of the source account to the specified value.
/// </summary>
public class BumpSequenceOperationResponse : OperationResponse
{
    public override int TypeId => 11;

    /// <summary>
    ///     The new sequence number for the source account.
    /// </summary>
    [JsonPropertyName("bump_to")]
    public required long BumpTo { get; init; }
}