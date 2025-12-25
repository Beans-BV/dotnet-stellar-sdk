using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Part of the transaction result representing a refund payment.
/// </summary>
public sealed class TransactionRefundPayment
{
    /// <summary>
    ///     The payment ID that can be used to identify the refund payment.
    ///     This is either a Stellar transaction hash or an off-chain payment identifier,
    ///     such as a reference number provided to the user when the refund was initiated.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    ///     stellar or external.
    /// </summary>
    [JsonPropertyName("id_type")]
    public required string IdType { get; init; }

    /// <summary>
    ///     The amount sent back to the user for the payment identified by id, in units of amount_in_asset.
    /// </summary>
    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }

    /// <summary>
    ///     The amount charged as a fee for processing the refund, in units of amount_in_asset.
    /// </summary>
    [JsonPropertyName("fee")]
    public required decimal Fee { get; init; }
}