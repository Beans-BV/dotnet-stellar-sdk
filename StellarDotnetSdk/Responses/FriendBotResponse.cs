using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents the response from the Friendbot service on testnet.
///     Friendbot is a service that funds accounts with test XLM on the Stellar testnet.
/// </summary>
public sealed class FriendBotResponse : Response
{
    /// <summary>
    ///     Links to related resources.
    /// </summary>
    [JsonPropertyName("_links")]
    public FriendBotResponseLinks? Links { get; init; }

    /// <summary>
    ///     The type of response (used for error responses).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>
    ///     The title of the response (used for error responses).
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    ///     The HTTP status code of the response.
    /// </summary>
    [JsonPropertyName("status")]
    public int? Status { get; init; }

    /// <summary>
    ///     Additional transaction details if the funding transaction failed.
    /// </summary>
    [JsonPropertyName("extras")]
    public SubmitTransactionResponse.Extras? Extras { get; init; }

    /// <summary>
    ///     Detailed description of the response or error.
    /// </summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    /// <summary>
    ///     The hash of the funding transaction.
    /// </summary>
    [JsonPropertyName("hash")]
    public string? Hash { get; init; }

    /// <summary>
    ///     The ledger sequence number in which the funding transaction was included.
    /// </summary>
    [JsonPropertyName("ledger")]
    public long Ledger { get; init; }

    /// <summary>
    ///     The XDR-encoded transaction envelope.
    /// </summary>
    [JsonPropertyName("envelope_xdr")]
    public string? EnvelopeXdr { get; init; }

    /// <summary>
    ///     The XDR-encoded transaction result.
    /// </summary>
    [JsonPropertyName("result_xdr")]
    public string? ResultXdr { get; init; }

    /// <summary>
    ///     The XDR-encoded transaction result metadata.
    /// </summary>
    [JsonPropertyName("result_meta_xdr")]
    public string? ResultMetaXdr { get; init; }

    /// <summary>
    ///     Links associated with the Friendbot response.
    /// </summary>
    public sealed class FriendBotResponseLinks
    {
        /// <summary>
        ///     Link to the funding transaction details.
        /// </summary>
        [JsonPropertyName("transaction")]
        public Link<TransactionResponse>? Transaction { get; init; }
    }
}