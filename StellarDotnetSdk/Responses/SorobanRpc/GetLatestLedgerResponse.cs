namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Represents the response from the Soroban RPC <c>getLatestLedger</c> method.
///     Contains the hash, sequence number, protocol version, close time, and encoded header and metadata of the most
///     recent ledger.
/// </summary>
public class GetLatestLedgerResponse
{
    /// <summary>
    ///     Hash identifier of the latest ledger (as a hex-encoded string) known to Stellar RPC at the time it handled the
    ///     request.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    ///     Stellar Core protocol version associated with the latest ledger.
    /// </summary>
    public int ProtocolVersion { get; init; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Stellar RPC at the time it handled the request.
    /// </summary>
    public int Sequence { get; init; }

    /// <summary>
    ///     The unix timestamp of the close time of the latest ledger, encoded as a string.
    /// </summary>
    public string? CloseTime { get; init; }

    /// <summary>
    ///     The base-64 encoded XDR of the latest ledger's header.
    /// </summary>
    public string? HeaderXdr { get; init; }

    /// <summary>
    ///     The base-64 encoded XDR of the latest ledger's close metadata.
    /// </summary>
    public string? MetadataXdr { get; init; }
}