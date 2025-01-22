using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Abstract class for operation responses.
///     See: https://www.stellar.org/developers/horizon/reference/resources/operation.html
///     <seealso cref="Requests.OperationsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
[JsonConverter(typeof(OperationResponseJsonConverter))]
public abstract class OperationResponse : Response, IPagingToken
{
    /// <summary>
    ///     ID of the operation
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; init; }

    /// <summary>
    ///     Source Account of Operation
    /// </summary>
    [JsonPropertyName("source_account")]
    public string SourceAccount { get; init; }

    [JsonPropertyName("source_account_muxed")]
    public string SourceAccountMuxed { get; init; }

    [JsonPropertyName("source_account_muxed_id")]
    public ulong? SourceAccountMuxedId { get; init; }

    /// <summary>
    ///     Returns operation type. Possible types:
    ///     create_account
    ///     payment
    ///     allow_trust
    ///     change_trust
    ///     set_options
    ///     account_merge
    ///     manage_offer
    ///     path_payments
    ///     create_passive_offer
    ///     inflation
    ///     manage_data
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; }

    [JsonPropertyName("type_i")]
    public virtual int TypeId { get; }

    /// <summary>
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    ///     Returns transaction hash of transaction this operation belongs to.
    /// </summary>
    [JsonPropertyName("transaction_hash")]
    public string TransactionHash { get; init; }

    /// <summary>
    ///     Returns whether the operation transaction was successful.
    /// </summary>
    [JsonPropertyName("transaction_successful")]
    public bool TransactionSuccessful { get; init; } = true;

    /// <summary>
    ///     Links of Paging
    /// </summary>
    [JsonPropertyName("_links")]
    public OperationResponseLinks Links { get; init; }
#nullable restore
    /// <summary>
    ///     Returns the transaction this operation belongs to.
    /// </summary>
    [JsonPropertyName("transaction")]
    public TransactionResponse? Transaction { get; init; }

    /// <summary>
    ///     Paging Token of Paging
    /// </summary>
    [JsonPropertyName("paging_token")]
    public string PagingToken { get; init; }
}