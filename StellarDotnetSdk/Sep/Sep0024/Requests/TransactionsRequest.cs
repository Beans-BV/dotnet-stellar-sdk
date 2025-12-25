using System;

namespace StellarDotnetSdk.Sep.Sep0024.Requests;

/// <summary>
///     Request to query transaction history for deposits and withdrawals.
///     This endpoint allows clients to fetch the status and history of transactions
///     with the anchor. It returns transactions associated with the account encoded
///     in the authenticated SEP-10 JWT token.
///     Authentication: Always required. Must provide a SEP-10 JWT token.
/// </summary>
public sealed record TransactionsRequest
{
    /// <summary>
    ///     Gets or sets the JWT token previously received from the anchor via the SEP-10 authentication flow.
    ///     Required for authentication.
    /// </summary>
    public required string Jwt { get; init; }

    /// <summary>
    ///     Gets or sets the code of the asset of interest (e.g., BTC, ETH, USD, INR).
    /// </summary>
    public required string AssetCode { get; init; }

    /// <summary>
    ///     Gets or sets the response should contain transactions starting on or after this date and time.
    ///     UTC ISO 8601 string format.
    /// </summary>
    public DateTime? NoOlderThan { get; init; }

    /// <summary>
    ///     Gets or sets the maximum number of transactions to return.
    ///     Used for pagination.
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    ///     Gets or sets the kind of transaction that is desired.
    ///     Should be either 'deposit' or 'withdrawal'.
    /// </summary>
    public string? Kind { get; init; }

    /// <summary>
    ///     Gets or sets the response should contain transactions starting prior to this ID (exclusive).
    ///     Used for pagination with the transaction ID.
    /// </summary>
    public string? PagingId { get; init; }

    /// <summary>
    ///     Gets or sets the language code specified using RFC 4646 (e.g., en-US).
    ///     Defaults to 'en' if not specified or if the specified language is not supported.
    /// </summary>
    public string? Lang { get; init; }
}

