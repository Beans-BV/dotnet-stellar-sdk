using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Additional information from the anchor.
///     Contains optional messages or additional details that an anchor wants to
///     communicate to the user about their transaction.
/// </summary>
public sealed class ExtraInfo : Response
{
    /// <summary>
    ///     Optional message from the anchor.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }
}

