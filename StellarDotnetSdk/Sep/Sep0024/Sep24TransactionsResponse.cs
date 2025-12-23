using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0024;

/// <summary>
///     Response from the /transactions endpoint containing a list of transactions.
///     Returns transaction history for deposits and withdrawals associated with
///     the authenticated account. Supports pagination and filtering by asset,
///     transaction kind, and date range.
/// </summary>
public class Sep24TransactionsResponse : Response
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Sep24TransactionsResponse" /> class.
    /// </summary>
    /// <param name="transactions">List of transactions matching the request criteria.</param>
    [JsonConstructor]
    public Sep24TransactionsResponse(List<Sep24Transaction> transactions)
    {
        Transactions = transactions;
    }

    /// <summary>
    ///     Gets the list of transactions matching the request criteria.
    ///     May be empty if no transactions match the filters.
    /// </summary>
    [JsonPropertyName("transactions")]
    public List<Sep24Transaction> Transactions { get; }
}

