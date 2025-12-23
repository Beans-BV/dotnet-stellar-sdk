using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Configuration for the anchor's transaction history endpoint.
///     Indicates whether the anchor provides the /transactions endpoint for querying
///     a list of transactions with filtering and pagination.
/// </summary>
public sealed class AnchorTransactionsInfo : Response
{
    /// <summary>
    ///     True if the endpoint is available.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; init; }

    /// <summary>
    ///     True if client must be authenticated before accessing the endpoint.
    /// </summary>
    [JsonPropertyName("authentication_required")]
    public bool? AuthenticationRequired { get; init; }
}

