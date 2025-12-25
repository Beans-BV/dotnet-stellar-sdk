using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Response indicating the status of customer information processing.
///     When customer information has been submitted but is still being reviewed
///     or has been rejected, this response provides the current status and
///     additional details.
/// </summary>
public sealed record CustomerInformationStatusResponse
{
    /// <summary>
    ///     Status of customer information processing. One of: pending, denied.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>
    ///     A URL the user can visit if they want more information about their account / status.
    /// </summary>
    [JsonPropertyName("more_info_url")]
    public string? MoreInfoUrl { get; init; }

    /// <summary>
    ///     Estimated number of seconds until the customer information status will update.
    /// </summary>
    [JsonPropertyName("eta")]
    public int? Eta { get; init; }
}