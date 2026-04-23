using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0045;

/// <summary>Response from a SEP-45 WebAuth server GET /auth endpoint.</summary>
public sealed record ChallengeForContractsResponse
{
    /// <summary>Base64-encoded XDR of the SorobanAuthorizationEntry list to sign.</summary>
    [JsonPropertyName("authorization_entries")]
    public string? AuthorizationEntries { get; init; }

    /// <summary>Network passphrase the challenge is bound to.</summary>
    [JsonPropertyName("network_passphrase")]
    public string? NetworkPassphrase { get; init; }
}
