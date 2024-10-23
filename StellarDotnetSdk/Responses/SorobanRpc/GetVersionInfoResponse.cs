namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Version information about the RPC and Captive core. RPC manages its own, pared-down version of Stellar Core
///     optimized for its own subset of needs. we'll refer to this as a "Captive Core" instance.
/// </summary>
public class GetVersionInfoResponse
{
    public GetVersionInfoResponse(
        string buildTimeStamp,
        string captiveCoreVersion,
        string commitHash,
        int protocolVersion,
        string version)
    {
        BuildTimeStamp = buildTimeStamp;
        CaptiveCoreVersion = captiveCoreVersion;
        CommitHash = commitHash;
        ProtocolVersion = protocolVersion;
        Version = version;
    }

    /// <summary>
    ///     The build timestamp of the RPC server.
    /// </summary>
    public string BuildTimeStamp { get; }

    /// <summary>
    ///     The version of the Captive Core.
    /// </summary>
    public string CaptiveCoreVersion { get; }

    /// <summary>
    ///     The commit hash of the RPC server.
    /// </summary>
    public string CommitHash { get; }

    /// <summary>
    ///     The protocol version.
    /// </summary>
    public int ProtocolVersion { get; }

    /// <summary>
    ///     The version of the RPC server.
    /// </summary>
    public string Version { get; }
}