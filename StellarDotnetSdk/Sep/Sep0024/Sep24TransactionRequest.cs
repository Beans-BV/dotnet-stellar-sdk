namespace StellarDotnetSdk.Sep.Sep0024;

/// <summary>
///     Request to query or validate a specific transaction with the anchor.
///     This endpoint allows clients to retrieve detailed information about a single
///     transaction. The anchor must verify that the SEP-10 JWT includes the Stellar
///     account (and optional memo) used when making the original deposit/withdraw request.
///     At least one of Id, StellarTransactionId, or ExternalTransactionId must be provided.
///     Authentication: Always required. Must provide a SEP-10 JWT token.
/// </summary>
public sealed record Sep24TransactionRequest
{
    /// <summary>
    ///     Gets or sets the JWT token previously received from the anchor via the SEP-10 authentication flow.
    ///     Required for authentication.
    /// </summary>
    public required string Jwt { get; init; }

    /// <summary>
    ///     Gets or sets the anchor's internal ID for the transaction.
    ///     This is the ID returned in the Sep24InteractiveResponse.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    ///     Gets or sets the Stellar transaction hash of the transaction on the Stellar network.
    /// </summary>
    public string? StellarTransactionId { get; init; }

    /// <summary>
    ///     Gets or sets the external transaction ID from the off-chain payment system.
    /// </summary>
    public string? ExternalTransactionId { get; init; }

    /// <summary>
    ///     Gets or sets the language code specified using RFC 4646 (e.g., en-US).
    ///     Defaults to 'en' if not specified or if the specified language is not supported.
    /// </summary>
    public string? Lang { get; init; }
}

