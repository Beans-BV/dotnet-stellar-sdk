using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Sep.Sep0012.Responses;

/// <summary>
///     Metadata of a file uploaded with <c>POST /customer/files</c>. Returned by that endpoint and, as list items, by
///     <c>GET /customer/files</c>.
/// </summary>
public sealed class CustomerFileResponse
{
    /// <summary>
    ///     Gets the unique identifier of the file. Reference it in <c>PUT /customer</c> under the SEP-0009 field name
    ///     suffixed with <c>_file_id</c> (see <see cref="Requests.PutCustomerInfoRequest.FileReferences" />).
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("file_id")]
    public string FileId { get; init; } = null!;

    /// <summary>
    ///     Gets the <c>Content-Type</c> of the file.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("content_type")]
    public string ContentType { get; init; } = null!;

    /// <summary>
    ///     Gets the size of the file in bytes.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("size")]
    public long Size { get; init; }

    /// <summary>
    ///     Gets the time at which the server discards the file if no <c>PUT /customer</c> request references it, or
    ///     <c>null</c> when the server did not say. SEP-0012 sends it as a UTC ISO 8601 string; a value without a zone
    ///     designator is read as UTC, never in the local time zone. More than seven fractional-second digits are
    ///     rounded to the nearest tick, and a value outside the ISO 8601 grammar, such as a fraction separator without
    ///     digits, fails the parse.
    /// </summary>
    [JsonPropertyName("expires_at")]
    [JsonConverter(typeof(UtcDateTimeOffsetJsonConverter))]
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    ///     Gets the ID of the customer the file is associated with, or <c>null</c> if the customer record does not
    ///     exist yet.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string? CustomerId { get; init; }
}
