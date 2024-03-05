using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.sorobanrpc;

/// <summary>
///     General information about the currently configured network. This response will contain all the information needed
///     to successfully submit transactions to the network this node serves.
///     See https://soroban.stellar.org/api/methods/getNetwork
/// </summary>
public class GetNetworkResponse
{
#nullable disable
    /// <summary>
    ///     (optional) The URL of this network's "friendbot" faucet.
    /// </summary>
    [JsonProperty] public readonly string FriendbotUrl;

    /// <summary>
    ///     Network passphrase configured for this Soroban RPC node.
    /// </summary>
    [JsonProperty] public readonly string Passphrase;

    /// <summary>
    ///     Stellar Core protocol version associated with the latest ledger.
    /// </summary>
    [JsonProperty] public readonly int ProtocolVersion;
}