using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Represents a transfer service fee response.
/// </summary>
public sealed class FeeResponse : Response
{
    /// <summary>
    ///     The total fee (in units of the asset involved) that would be charged
    ///     to deposit/withdraw the specified amount of asset_code.
    /// </summary>
    [JsonPropertyName("fee")]
    public required decimal Fee { get; init; }
}

