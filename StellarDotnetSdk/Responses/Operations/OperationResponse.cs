using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Base class for all operation responses.
///     Operations are actions that mutate the ledger, such as creating accounts or making payments.
///     See:
///     <a href="https://developers.stellar.org/docs/data/apis/horizon/api-reference/resources/operations">Operations</a>
/// </summary>
/// <remarks>
///     <para>
///         <seealso cref="Requests.OperationsRequestBuilder" />
///     </para>
///     <para>
///         <seealso cref="Server" />
///     </para>
/// </remarks>
[JsonConverter(typeof(OperationResponseJsonConverter))]
public abstract class OperationResponse : Response, IPagingToken
{
    /// <summary>
    ///     The unique identifier for this operation.
    /// </summary>
    [JsonPropertyName("id")]
    public required long Id { get; init; }

    /// <summary>
    ///     The account that originates the operation.
    /// </summary>
    [JsonPropertyName("source_account")]
    public required string SourceAccount { get; init; }

    /// <summary>
    ///     The muxed account representation of the source account, if applicable.
    /// </summary>
    [JsonPropertyName("source_account_muxed")]
    public string? SourceAccountMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the source account, if applicable.
    /// </summary>
    [JsonPropertyName("source_account_muxed_id")]
    public ulong? SourceAccountMuxedId { get; init; }

    /// <summary>
    ///     The operation type name (e.g., "payment", "create_account", "manage_offer").
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    ///     The numeric identifier for the operation type.
    /// </summary>
    [JsonPropertyName("type_i")]
    public abstract int TypeId { get; }

    /// <summary>
    ///     The date and time this operation was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     The hash of the transaction that contains this operation.
    /// </summary>
    [JsonPropertyName("transaction_hash")]
    public required string TransactionHash { get; init; }

    /// <summary>
    ///     Indicates whether the transaction containing this operation was successful.
    /// </summary>
    [JsonPropertyName("transaction_successful")]
    public bool TransactionSuccessful { get; init; } = true;

    /// <summary>
    ///     Navigation links to related resources.
    /// </summary>
    [JsonPropertyName("_links")]
    public required OperationResponseLinks Links { get; init; }

    /// <summary>
    ///     The transaction that contains this operation.
    ///     Only present when the operation is retrieved with the transaction included.
    /// </summary>
    [JsonPropertyName("transaction")]
    public TransactionResponse? Transaction { get; init; }

    /// <summary>
    ///     A cursor value for use in pagination.
    /// </summary>
    [JsonPropertyName("paging_token")]
    public required string PagingToken { get; init; }
}