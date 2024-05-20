namespace StellarDotnetSdk.Responses.SorobanRpc;

public class GetLatestLedgerResponse
{
    public GetLatestLedgerResponse(string id, int protocolVersion, int sequence)
    {
        Id = id;
        ProtocolVersion = protocolVersion;
        Sequence = sequence;
    }

    /// <summary>
    ///     Hash identifier of the latest ledger (as a hex-encoded string) known to Soroban RPC at the time it handled the
    ///     request.
    /// </summary>
    public string Id { get; }

    /// <summary>
    ///     Stellar Core protocol version associated with the latest ledger.
    /// </summary>
    public int ProtocolVersion { get; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public int Sequence { get; }
}