using System;

namespace StellarDotnetSdk.Sep.Sep0006.Requests;

/// <summary>
///     Request parameters for querying a list of transactions.
///     This class encapsulates parameters for retrieving a filtered and paginated
///     list of deposit and withdrawal transactions associated with a specific account
///     and asset.
/// </summary>
public sealed record AnchorTransactionsRequest
{
    /// <summary>
    ///     The code of the asset of interest. E.g. BTC, ETH, USD, INR, etc.
    /// </summary>
    public required string AssetCode { get; init; }

    /// <summary>
    ///     The stellar account ID involved in the transactions. If the service
    ///     requires SEP-10 authentication, this parameter must match the
    ///     authenticated account.
    /// </summary>
    public required string Account { get; init; }

    /// <summary>
    ///     The response should contain transactions starting on or
    ///     after this date & time.
    /// </summary>
    public DateTime? NoOlderThan { get; init; }

    /// <summary>
    ///     The response should contain at most limit transactions.
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    ///     A list containing the desired transaction kinds.
    ///     The possible values are deposit, deposit-exchange, withdrawal
    ///     and withdrawal-exchange.
    /// </summary>
    public string? Kind { get; init; }

    /// <summary>
    ///     The response should contain transactions starting
    ///     prior to this ID (exclusive).
    /// </summary>
    public string? PagingId { get; init; }

    /// <summary>
    ///     Defaults to en if not specified. Language code specified using RFC 4646.
    /// </summary>
    public string? Lang { get; init; }

    /// <summary>
    ///     JWT previously received from the anchor via the SEP-10 authentication flow.
    /// </summary>
    public string? Jwt { get; init; }
}