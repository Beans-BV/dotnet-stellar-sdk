namespace stellar_dotnet_sdk.responses.sorobanrpc;

public class GetNetworkResponse
{
    public string FriendbotUrl { get; set; }

    public string Passphrase { get; set; }

    public int ProtocolVersion { get; set; }
}