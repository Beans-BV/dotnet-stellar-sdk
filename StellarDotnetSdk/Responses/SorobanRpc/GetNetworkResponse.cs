namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     General information about the currently configured network. This response will contain all the information needed
///     to successfully submit transactions to the network this node serves.
///     See https://developers.stellar.org/docs/data/rpc/api-reference/methods/getNetwork
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
    public string FriendbotUrl { get; }

    /// <summary>
    ///     Network passphrase configured for this Soroban RPC node.
    /// </summary>
    public string Passphrase { get; }

    /// <summary>
    ///     Stellar Core protocol version associated with the latest ledger.
    /// </summary>
    public int ProtocolVersion { get; }
}