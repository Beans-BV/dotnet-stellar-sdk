using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Response from the GET /transaction endpoint for a specific transaction.
///     Returns detailed information about a single transaction identified by its ID,
///     stellar transaction ID, or external transaction ID.
/// </summary>
public sealed class AnchorTransactionResponse : Response
{
    /// <summary>
    ///     The transaction details.
    /// </summary>
    [JsonPropertyName("transaction")]
    public required AnchorTransaction Transaction { get; init; }
}

