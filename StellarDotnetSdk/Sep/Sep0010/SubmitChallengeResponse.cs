using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0010;

/// <summary>
///     Response from the SEP-0010 token endpoint after submitting a signed challenge transaction.
/// </summary>
public sealed record SubmitChallengeResponse
{
    /// <summary>
    ///     The JWT authentication token returned upon successful challenge submission.
    ///     This token can be used to authenticate subsequent API requests.
    /// </summary>
    [JsonPropertyName("token")]
    public string? Token { get; init; }

    /// <summary>
    ///     Error message if the challenge submission failed.
    ///     Present when the server rejects the signed challenge transaction.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }
}

