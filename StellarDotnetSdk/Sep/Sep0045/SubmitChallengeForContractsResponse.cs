using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0045;

/// <summary>Response from a SEP-45 WebAuth server POST /auth endpoint.</summary>
public sealed record SubmitChallengeForContractsResponse
{
    /// <summary>JWT session token returned on successful authentication.</summary>
    [JsonPropertyName("token")]
    public string? Token { get; init; }

    /// <summary>Error message returned when authentication fails.</summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }
}
