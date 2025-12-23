using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Configuration for the anchor's single transaction query endpoint.
///     Indicates whether the anchor provides the /transaction endpoint for querying
///     details about a specific transaction by ID.
/// </summary>
public sealed class AnchorTransactionInfo : Response
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

