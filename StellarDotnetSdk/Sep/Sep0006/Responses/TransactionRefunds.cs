using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Part of the transaction result representing refund information.
/// </summary>
public sealed record TransactionRefunds
{
    /// <summary>
    ///     The total amount refunded to the user, in units of amount_in_asset.
    ///     If a full refund was issued, this amount should match amount_in.
    /// </summary>
    [JsonPropertyName("amount_refunded")]
    public required decimal AmountRefunded { get; init; }

    /// <summary>
    ///     The total amount charged in fees for processing all refund payments, in units of amount_in_asset.
    ///     The sum of all fee values in the payments object list should equal this value.
    /// </summary>
    [JsonPropertyName("amount_fee")]
    public required decimal AmountFee { get; init; }

    /// <summary>
    ///     A list of objects containing information on the individual payments made back to the user as refunds.
    /// </summary>
    [JsonPropertyName("payments")]
    public required IReadOnlyList<TransactionRefundPayment> Payments { get; init; }
}