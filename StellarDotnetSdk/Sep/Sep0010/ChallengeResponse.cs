using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0010;

/// <summary>
///     Response from the SEP-0010 challenge endpoint containing the challenge transaction.
/// </summary>
public sealed record ChallengeResponse
{
    /// <summary>
    ///     The base64-encoded XDR transaction envelope that must be signed and submitted.
    /// </summary>
    [JsonPropertyName("transaction")]
    public string? Transaction { get; init; }

    /// <summary>
    ///     Optional network passphrase for the challenge transaction.
    ///     If not provided, the client should use the network passphrase from stellar.toml.
    /// </summary>
    [JsonPropertyName("network_passphrase")]
    public string? NetworkPassphrase { get; init; }
}