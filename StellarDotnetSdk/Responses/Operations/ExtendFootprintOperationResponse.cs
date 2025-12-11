using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents an extend_footprint_ttl operation response.
///     Extends the time-to-live (TTL) of Soroban contract data entries.
/// </summary>
public class ExtendFootprintOperationResponse : OperationResponse
{
    public override int TypeId => 25;

    /// <summary>
    ///     The number of ledgers to extend the TTL by.
    /// </summary>
    [JsonPropertyName("extend_to")]
    public required int ExtendTo { get; init; }
}