using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0024;

/// <summary>
///     Information about a single refund payment.
///     Represents an individual payment made back to the user as part of a refund.
///     Multiple refund payments may exist for a single transaction.
/// </summary>
public class Sep24RefundPayment : Response
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Sep24RefundPayment" /> class.
    /// </summary>
    /// <param name="id">Payment ID (Stellar transaction hash or off-chain reference).</param>
    /// <param name="idType">Type of refund payment ('stellar' or 'external').</param>
    /// <param name="amount">Amount sent back to user in units of amountInAsset.</param>
    /// <param name="fee">Fee charged for processing this refund payment.</param>
    [JsonConstructor]
    public Sep24RefundPayment(string id, string idType, string amount, string fee)
    {
        Id = id;
        IdType = idType;
        Amount = amount;
        Fee = fee;
    }

    /// <summary>
    ///     Gets the payment ID that can be used to identify the refund payment.
    ///     This is either a Stellar transaction hash or an off-chain payment identifier
    ///     (such as a reference number provided when the refund was initiated).
    ///     This ID is not guaranteed to be unique.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; }

    /// <summary>
    ///     Gets the type of refund payment: 'stellar' or 'external'.
    ///     Indicates whether the refund was made on the Stellar network or via an external system.
    /// </summary>
    [JsonPropertyName("id_type")]
    public string IdType { get; }

    /// <summary>
    ///     Gets the amount sent back to the user for this payment, in units of amountInAsset.
    /// </summary>
    [JsonPropertyName("amount")]
    public string Amount { get; }

    /// <summary>
    ///     Gets the fee charged for processing this refund payment, in units of amountInAsset.
    /// </summary>
    [JsonPropertyName("fee")]
    public string Fee { get; }
}

