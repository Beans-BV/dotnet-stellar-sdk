using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0024;

/// <summary>
///     Response from the /transaction endpoint containing a single transaction.
///     Returns detailed information about a specific transaction identified by
///     its ID, Stellar transaction hash, or external transaction ID.
/// </summary>
public class Sep24TransactionResponse : Response
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Sep24TransactionResponse" /> class.
    /// </summary>
    /// <param name="transaction">The transaction details queried from the anchor.</param>
    [JsonConstructor]
    public Sep24TransactionResponse(Sep24Transaction transaction)
    {
        Transaction = transaction;
    }

    /// <summary>
    ///     Gets the transaction details.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Sep24Transaction Transaction { get; }
}

