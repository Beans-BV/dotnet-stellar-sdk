using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Response indicating additional customer information is needed.
///     When an anchor needs more KYC information before processing a transaction,
///     this response specifies which fields must be provided via SEP-12.
/// </summary>
public sealed class CustomerInformationNeededResponse
{
    /// <summary>
    ///     A list of field names that need to be transmitted via SEP-12 for the transaction to proceed.
    /// </summary>
    [JsonPropertyName("fields")]
    public IReadOnlyList<string>? Fields { get; init; }
}

