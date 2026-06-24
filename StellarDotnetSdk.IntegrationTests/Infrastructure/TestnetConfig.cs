using System;

namespace StellarDotnetSdk.IntegrationTests.Infrastructure;

/// <summary>
///     Central, environment-overridable configuration for the integration tests.
///     Defaults point at the public Stellar Testnet; CI may override via env vars
///     (e.g. to route the data plane through an authenticated provider like Blockdaemon).
/// </summary>
public static class TestnetConfig
{
    /// <summary>Horizon base URL for all reads and submissions. Override to a provider endpoint.</summary>
    public static string HorizonUrl => Env("INTEGRATION_HORIZON_URL", "https://horizon-testnet.stellar.org");

    /// <summary>
    ///     Optional bearer token for <see cref="HorizonUrl" /> (e.g. a Blockdaemon API key).
    ///     Null when unset, in which case requests are unauthenticated.
    /// </summary>
    public static string? HorizonToken => EnvOrNull("INTEGRATION_HORIZON_TOKEN");

    /// <summary>
    ///     Horizon base URL used only for Friendbot funding. Defaults to Stellar's public Testnet,
    ///     which hosts the Friendbot faucet; most providers (Blockdaemon included) do not, so funding
    ///     stays here even when <see cref="HorizonUrl" /> points elsewhere.
    /// </summary>
    public static string FriendbotUrl => Env("INTEGRATION_FRIENDBOT_URL", "https://horizon-testnet.stellar.org");

    /// <summary>
    ///     Stellar RPC (formerly Soroban RPC) base URL. Consumed by Soroban tests in a later phase.
    ///     The public Testnet host is still <c>soroban-testnet.stellar.org</c> despite the renamed term.
    /// </summary>
    public static string StellarRpcUrl => Env("INTEGRATION_STELLAR_RPC_URL", "https://soroban-testnet.stellar.org");

    /// <summary>Optional bearer token for <see cref="StellarRpcUrl" />. Null when unset.</summary>
    public static string? StellarRpcToken => EnvOrNull("INTEGRATION_STELLAR_RPC_TOKEN");

    public static string NetworkPassphrase => Network.TestnetPassphrase;

    private static string Env(string key, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static string? EnvOrNull(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}