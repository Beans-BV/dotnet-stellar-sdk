using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Links to related resources for a transaction.
/// </summary>
public sealed class TransactionResponseLinks
{
    /// <summary>
    ///     Link to the source account of this transaction.
    /// </summary>
    [JsonPropertyName("account")]
    public required Link<AccountResponse> Account { get; init; }

    /// <summary>
    ///     Link to effects created by this transaction.
    /// </summary>
    [JsonPropertyName("effects")]
    public required Link<Page<EffectResponse>> Effects { get; init; }

    /// <summary>
    ///     Link to the ledger containing this transaction.
    /// </summary>
    [JsonPropertyName("ledger")]
    public required Link<LedgerResponse> Ledger { get; init; }

    /// <summary>
    ///     Link to operations in this transaction.
    /// </summary>
    [JsonPropertyName("operations")]
    public required Link<Page<OperationResponse>> Operations { get; init; }

    /// <summary>
    ///     Link to the next page of transactions in the ledger.
    /// </summary>
    [JsonPropertyName("precedes")]
    public required Link<Page<TransactionResponse>> Precedes { get; init; }

    /// <summary>
    ///     Link to this transaction resource.
    /// </summary>
    [JsonPropertyName("self")]
    public required Link<TransactionResponse> Self { get; init; }

    /// <summary>
    ///     Link to the previous transaction in the ledger.
    /// </summary>
    [JsonPropertyName("succeeds")]
    public required Link<Page<TransactionResponse>> Succeeds { get; init; }

    /// <summary>
    ///     Link to the transaction in the ledger.
    /// </summary>
    [JsonPropertyName("transaction")]
    public required Link<TransactionResponse> Transaction { get; init; }
}