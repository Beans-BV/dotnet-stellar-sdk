using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.sorobanrpc;

/// <summary>
///     General information about the currently configured network. This response will contain all the information needed
///     to successfully submit transactions to the network this node serves.
///     See https://soroban.stellar.org/api/methods/getNetwork
/// </summary>
public class GetNetworkResponse
{
    public GetNetworkResponse(string friendbotUrl, string passphrase, int protocolVersion)
    {
        FriendbotUrl = friendbotUrl;
        Passphrase = passphrase;
        ProtocolVersion = protocolVersion;
    }

    /// <summary>
    ///     (optional) The URL of this network's "friendbot" faucet.
    /// </summary>
    [JsonProperty]
    public string FriendbotUrl { get; }

    /// <summary>
    ///     Network passphrase configured for this Soroban RPC node.
    /// </summary>
    [JsonProperty]
    public string Passphrase { get; }

    /// <summary>
    ///     Stellar Core protocol version associated with the latest ledger.
    /// </summary>
    [JsonProperty]
    public int ProtocolVersion { get; }
}