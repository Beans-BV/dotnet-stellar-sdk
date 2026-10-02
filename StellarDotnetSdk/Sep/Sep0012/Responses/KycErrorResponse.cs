using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0012.Responses;

/// <summary>
///     Body of a SEP-0012 error response. Parsed through the typed model — rather than by reading a
///     <see cref="System.Text.Json.JsonDocument" /> — so that <c>AllowDuplicateProperties = false</c> in
///     <see cref="StellarDotnetSdk.Converters.JsonOptions.DefaultOptions" /> rejects a body that repeats
///     <c>error</c> or <c>type</c> instead of silently keeping the last value (see issue #205).
/// </summary>
internal sealed class KycErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }
}
