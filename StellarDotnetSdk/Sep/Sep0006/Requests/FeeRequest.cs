namespace StellarDotnetSdk.Sep.Sep0006.Requests;

/// <summary>
///     Request parameters for querying transaction fees.
///     This class encapsulates the parameters needed to query the fee that an anchor
///     would charge for a specific deposit or withdrawal operation.
/// </summary>
public sealed record FeeRequest
{
    /// <summary>
    ///     Kind of operation (deposit or withdraw).
    /// </summary>
    public required string Operation { get; init; }

    /// <summary>
    ///     Stellar asset code.
    /// </summary>
    public required string AssetCode { get; init; }

    /// <summary>
    ///     Amount of the asset that will be deposited/withdrawn.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    ///     Type of deposit or withdrawal (SEPA, bank_account, cash, etc...).
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    ///     JWT previously received from the anchor via the SEP-10 authentication flow.
    /// </summary>
    public string? Jwt { get; init; }
}

