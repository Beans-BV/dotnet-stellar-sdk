using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a manage_data operation response.
///     Sets, modifies, or deletes a data entry (key-value pair) associated with an account.
/// </summary>
public class ManageDataOperationResponse : OperationResponse
{
    public override int TypeId => 10;

    /// <summary>
    ///     The name (key) of the data entry.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    ///     The value of the data entry (base64 encoded). Null if the data entry is being deleted.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; init; }
}