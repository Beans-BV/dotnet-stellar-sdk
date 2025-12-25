using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Container for required info updates in a transaction.
///     The SEP-6 API wraps the fields in a "transaction" object.
/// </summary>
public sealed class RequiredInfoUpdates
{
    /// <summary>
    ///     The fields that require updates, keyed by field name.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Dictionary<string, AnchorField>? Transaction { get; init; }
}

