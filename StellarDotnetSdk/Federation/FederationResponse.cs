using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Federation;
#nullable disable

/// <summary>
///     Object to hold a response from a federation server.
///     See
///     <a href="https://developers.stellar.org/docs/learn/encyclopedia/federation#federation-response">Federation response</a>
/// </summary>
public class FederationResponse
{
    [JsonPropertyName("stellar_address")]
    public string StellarAddress { get; init; }

    [JsonPropertyName("account_id")]
    public string AccountId { get; init; }

    [JsonPropertyName("memo_type")]
    public string MemoType { get; init; }

    [JsonPropertyName("memo")]
    public string Memo { get; init; }
}