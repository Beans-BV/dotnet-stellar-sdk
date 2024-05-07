using Newtonsoft.Json;

namespace StellarDotnetSdk.Federation;
#nullable disable

/// <summary>
///     Object to hold a response from a federation server.
///     See
///     <a href="https://developers.stellar.org/docs/learn/encyclopedia/federation#federation-response">Federation response</a>
/// </summary>
public class FederationResponse
{
    [JsonProperty(PropertyName = "stellar_address")]
    public string StellarAddress { get; init; }

    [JsonProperty(PropertyName = "account_id")]
    public string AccountId { get; init; }

    [JsonProperty(PropertyName = "memo_type")]
    public string MemoType { get; init; }

    [JsonProperty(PropertyName = "memo")] public string Memo { get; init; }
}