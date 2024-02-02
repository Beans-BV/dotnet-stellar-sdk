using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.sorobanrpc;

public class GetLatestLedgerResponse
{
    public string Id;

    public int ProtocolVersion;

    public int Sequence;
}