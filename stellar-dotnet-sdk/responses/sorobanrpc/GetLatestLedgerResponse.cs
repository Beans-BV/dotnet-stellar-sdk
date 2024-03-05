using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.sorobanrpc;

[JsonObject]
public class GetLatestLedgerResponse
{
    /// <summary>
    ///     Hash identifier of the latest ledger (as a hex-encoded string) known to Soroban RPC at the time it handled the
    ///     request.
    /// </summary>
    [JsonProperty("id")] public readonly string Id;

    /// <summary>
    ///     Stellar Core protocol version associated with the latest ledger.
    /// </summary>
    [JsonProperty("protocolVersion")] public readonly int ProtocolVersion;

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    [JsonProperty("sequence")] public readonly int Sequence;

    public GetLatestLedgerResponse(string id, int protocolVersion, int sequence)
    {
        Id = id;
        ProtocolVersion = protocolVersion;
        Sequence = sequence;
    }
}