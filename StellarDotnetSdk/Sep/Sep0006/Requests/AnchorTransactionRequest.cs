namespace StellarDotnetSdk.Sep.Sep0006.Requests;

/// <summary>
///     Request parameters for querying a specific transaction.
///     This class encapsulates parameters for retrieving detailed information about
///     a single transaction. The transaction can be identified using one of three
///     possible identifiers: the anchor's transaction ID, the Stellar transaction ID,
///     or an external transaction ID.
/// </summary>
public sealed record AnchorTransactionRequest
{
    /// <summary>
    ///     The id of the transaction.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    ///     The stellar transaction id of the transaction.
    /// </summary>
    public string? StellarTransactionId { get; init; }

    /// <summary>
    ///     The external transaction id of the transaction.
    /// </summary>
    public string? ExternalTransactionId { get; init; }

    /// <summary>
    ///     Defaults to en if not specified. Language code specified using RFC 4646.
    /// </summary>
    public string? Lang { get; init; }

    /// <summary>
    ///     JWT previously received from the anchor via the SEP-10 authentication flow.
    /// </summary>
    public string? Jwt { get; init; }
}

