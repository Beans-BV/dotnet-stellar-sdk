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
    /// <summary>Gets the Stellar address (e.g., <c>bob*stellar.org</c>).</summary>
    [JsonPropertyName("stellar_address")]
    public string StellarAddress { get; init; }

    /// <summary>Gets the Stellar account ID (G...) associated with the federation address.</summary>
    [JsonPropertyName("account_id")]
    public string AccountId { get; init; }

    /// <summary>Gets the memo type required for transactions to this account (e.g., "text", "id", "hash").</summary>
    [JsonPropertyName("memo_type")]
    public string MemoType { get; init; }

    /// <summary>Gets the memo value required for transactions to this account.</summary>
    [JsonPropertyName("memo")]
    public string Memo { get; init; }
}