using System;
using System.Net.Http;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Deprecated. Use <see cref="StellarRpcServer" /> instead.
/// </summary>
[Obsolete("Use StellarRpcServer instead. This class will be removed in a future version.")]
public class SorobanServer : StellarRpcServer
{
    /// <inheritdoc />
    public SorobanServer(string uri, HttpClient httpClient) : base(uri, httpClient)
    {
    }

    /// <inheritdoc />
    public SorobanServer(string uri, string? bearerToken = null) : base(uri, bearerToken)
    {
    }

    /// <inheritdoc />
    public SorobanServer(string uri, HttpResilienceOptions? resilienceOptions, string? bearerToken = null)
        : base(uri, resilienceOptions, bearerToken)
    {
    }
}
