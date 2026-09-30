using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0012.Responses;

/// <summary>
///     Response of <c>GET /customer/files</c>.
/// </summary>
public sealed class GetCustomerFilesResponse
{
    /// <summary>
    ///     Gets the matching files: a single entry when queried by <c>file_id</c>, every file uploaded for the
    ///     customer when queried by <c>customer_id</c>, or an empty array when nothing matches.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("files")]
    [JsonConverter(typeof(CustomerFilesJsonConverter))]
    public CustomerFileResponse[] Files { get; init; } = null!;
}
