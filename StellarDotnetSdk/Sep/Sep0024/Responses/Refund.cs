using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0024.Responses;

/// <summary>
///     Information about refunds associated with a transaction.
///     Contains details about on-chain or off-chain refunds issued for a transaction,
///     including total amounts, fees, and individual payment records.
/// </summary>
public class Refund : Response
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Refund" /> class.
    /// </summary>
    /// <param name="amountRefunded">The total amount refunded to the user, in units of amountInAsset.</param>
    /// <param name="amountFee">The total amount charged in fees for processing all refund payments.</param>
    /// <param name="payments">A list of individual refund payments made back to the user.</param>
    [JsonConstructor]
    public Refund(decimal amountRefunded, decimal amountFee, List<RefundPayment> payments)
    {
        AmountRefunded = amountRefunded;
        AmountFee = amountFee;
        Payments = payments;
    }

    /// <summary>
    ///     Gets the total amount refunded to the user, in units of amountInAsset.
    ///     If a full refund was issued, this amount should match the transaction's amountIn.
    /// </summary>
    [JsonPropertyName("amount_refunded")]
    public decimal AmountRefunded { get; }

    /// <summary>
    ///     Gets the total amount charged in fees for processing all refund payments,
    ///     in units of amountInAsset.
    ///     The sum of all fee values in the payments list should equal this value.
    /// </summary>
    [JsonPropertyName("amount_fee")]
    public decimal AmountFee { get; }

    /// <summary>
    ///     Gets a list of individual refund payments made back to the user.
    ///     Multiple payments may be issued for partial refunds or refund fee adjustments.
    /// </summary>
    [JsonPropertyName("payments")]
    public List<RefundPayment> Payments { get; }
}

