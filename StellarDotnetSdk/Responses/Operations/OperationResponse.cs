﻿using System.ComponentModel;
using Newtonsoft.Json;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Abstract class for operation responses.
///     See: https://www.stellar.org/developers/horizon/reference/resources/operation.html
///     <seealso cref="Requests.OperationsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
[JsonConverter(typeof(OperationDeserializer))]
public abstract class OperationResponse : Response, IPagingToken
{
    /// <summary>
    ///     Id of the operation
    /// </summary>
    [JsonProperty(PropertyName = "id")]
    public long Id { get; private set; }

    /// <summary>
    ///     Source Account of Operation
    /// </summary>
    [JsonProperty(PropertyName = "source_account")]
    public string SourceAccount { get; private set; }

    [JsonProperty(PropertyName = "source_account_muxed")]
    public string SourceAccountMuxed { get; private set; }

    [JsonProperty(PropertyName = "source_account_muxed_id")]
    public ulong? SourceAccountMuxedID { get; private set; }

    /// <summary>
    ///     Returns operation type. Possible types:
    ///     crete_account
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
    [JsonProperty(PropertyName = "type")]
    public string Type { get; private set; }

    [JsonProperty(PropertyName = "type_i")]
    public virtual int TypeId { get; }

    /// <summary>
    /// </summary>
    [JsonProperty(PropertyName = "created_at")]
    public string CreatedAt { get; private set; }

    /// <summary>
    ///     Returns transaction hash of transaction this operation belongs to.
    /// </summary>
    [JsonProperty(PropertyName = "transaction_hash")]
    public string TransactionHash { get; private set; }

    /// <summary>
    ///     Returns whether the operation transaction was successful.
    /// </summary>
    [DefaultValue(true)]
    [JsonProperty(PropertyName = "transaction_successful", DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool TransactionSuccessful { get; private set; }

    /// <summary>
    ///     Links of Paging
    /// </summary>
    [JsonProperty(PropertyName = "_links")]
    public OperationResponseLinks Links { get; private set; }

    /// <summary>
    ///     Returns the transaction this operation belongs to.
    /// </summary>
    [JsonProperty(PropertyName = "transaction")]
    public TransactionResponse? Transaction { get; private set; }

    /// <summary>
    ///     Paging Token of Paging
    /// </summary>
    [JsonProperty(PropertyName = "paging_token")]
    public string PagingToken { get; private set; }
}