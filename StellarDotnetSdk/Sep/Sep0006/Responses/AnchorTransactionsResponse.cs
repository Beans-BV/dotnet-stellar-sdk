using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Response from the GET /transactions endpoint containing transaction history.
///     Returns a list of transactions matching the query criteria (asset, account, etc.)
///     with pagination support.
/// </summary>
public sealed class AnchorTransactionsResponse : Response
{
    /// <summary>
    ///     List of transactions matching the query criteria.
    /// </summary>
    [JsonPropertyName("transactions")]
    public required IReadOnlyList<AnchorTransaction> Transactions { get; init; }
}
