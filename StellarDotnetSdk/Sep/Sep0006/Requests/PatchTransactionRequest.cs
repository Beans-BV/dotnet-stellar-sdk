using System.Collections.Generic;

namespace StellarDotnetSdk.Sep.Sep0006.Requests;

/// <summary>
///     Request parameters for updating transaction information.
///     This class encapsulates parameters for updating a transaction with additional
///     fields that the anchor has requested.
/// </summary>
public sealed record PatchTransactionRequest
{
    /// <summary>
    ///     Id of the transaction.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    ///     An object containing the values requested to be updated by the anchor.
    /// </summary>
    public Dictionary<string, object>? Fields { get; init; }

    /// <summary>
    ///     JWT previously received from the anchor via the SEP-10 authentication flow.
    /// </summary>
    public string? Jwt { get; init; }
}