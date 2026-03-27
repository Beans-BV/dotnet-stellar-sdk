namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Represents the details of a single ledger returned by <c>getLedgers()</c>.
/// </summary>
public class LedgerInfo
{
    /// <summary>
    ///     The hex-encoded hash of the ledger header.
    /// </summary>
    public string? Hash { get; init; }

    /// <summary>
    ///     The sequence number of the ledger.
    /// </summary>
    public long Sequence { get; init; }

    /// <summary>
    ///     The ISO 8601 timestamp at which the ledger was closed.
    /// </summary>
    public string? LedgerCloseTime { get; init; }

    /// <summary>
    ///     The base-64 encoded XDR of the ledger header.
    /// </summary>
    public string? HeaderXdr { get; init; }

    /// <summary>
    ///     The base-64 encoded XDR of the ledger close metadata.
    /// </summary>
    public string? MetadataXdr { get; init; }
}