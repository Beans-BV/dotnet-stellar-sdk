using System;

namespace StellarDotnetSdk.IntegrationTests.Infrastructure;

/// <summary>
///     Central, environment-overridable configuration for the integration tests.
///     Defaults point at the public Stellar Testnet; CI may override via env vars.
/// </summary>
public static class TestnetConfig
{
    public static string HorizonUrl => Env("INTEGRATION_HORIZON_URL", "https://horizon-testnet.stellar.org");
    public static string SorobanRpcUrl => Env("INTEGRATION_SOROBAN_RPC_URL", "https://soroban-testnet.stellar.org");
    public static string NetworkPassphrase => Network.TestnetPassphrase;

    private static string Env(string key, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}