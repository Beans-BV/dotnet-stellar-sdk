using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Configuration for the anchor's fee endpoint availability.
///     Indicates whether the anchor provides a dedicated /fee endpoint for querying
///     transaction fees dynamically.
/// </summary>
public sealed class AnchorFeeInfo : Response
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

    /// <summary>
    ///     Optional. Anchors are encouraged to add a description field to the
    ///     fee object returned in GET /info containing a short explanation of
    ///     how fees are calculated so client applications will be able to display
    ///     this message to their users.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}