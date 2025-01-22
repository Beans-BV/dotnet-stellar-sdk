using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Version information about the RPC and Captive core. RPC manages its own, pared-down version of Stellar Core
///     optimized for its own subset of needs. we'll refer to this as a "Captive Core" instance.
/// </summary>
public class GetVersionInfoResponse
{
    /// <summary>
    ///     The build timestamp of the RPC server.
    /// </summary>
    [JsonPropertyName("build_time_stamp")]
    public string BuildTimeStamp { get; init; }

    /// <summary>
    ///     The version of the Captive Core.
    /// </summary>
    [JsonPropertyName("captive_core_version")]
    public string CaptiveCoreVersion { get; init; }

    /// <summary>
    ///     The commit hash of the RPC server.
    /// </summary>
    [JsonPropertyName("commit_hash")]
    public string CommitHash { get; init; }

    /// <summary>
    ///     The protocol version.
    /// </summary>
    [JsonPropertyName("protocol_version")]
    public int ProtocolVersion { get; init; }

    /// <summary>
    ///     The version of the RPC server.
    /// </summary>
    public string Version { get; init; }
}