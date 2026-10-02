using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Sep.Sep0038.Responses;

/// <summary>
///     Response of SEP-38 <c>GET /info</c>: the Stellar and off-chain assets the anchor makes available for
///     trading.
/// </summary>
public sealed record InfoResponse
{
    /// <summary>
    ///     The assets available in exchange for one or more of the other assets listed.
    /// </summary>
    [JsonPropertyName("assets")]
    [JsonRequired]
    [JsonConverter(typeof(NonNullElementListJsonConverter<AssetInfo>))]
    public required IReadOnlyList<AssetInfo> Assets { get; init; }
}
