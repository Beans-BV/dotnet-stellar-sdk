namespace StellarDotnetSdk.Responses.SorobanRpc;

public class GetLatestLedgerResponse
{
    /// <summary>
    ///     Hash identifier of the latest ledger (as a hex-encoded string) known to Soroban RPC at the time it handled the
    ///     request.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    ///     Stellar Core protocol version associated with the latest ledger.
    /// </summary>
    public int ProtocolVersion { get; init; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public int Sequence { get; init; }
}