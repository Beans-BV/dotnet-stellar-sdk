using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace StellarDotnetSdk.IntegrationTests.Infrastructure;

[TestFixture]
[NonParallelizable] // mutates process environment variables
public class TestnetConfigTests
{
    [SetUp]
    public void SetUp()
    {
        foreach (var key in ManagedVars)
        {
            _originalValues[key] = Environment.GetEnvironmentVariable(key);
            Environment.SetEnvironmentVariable(key, null);
        }
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var key in ManagedVars)
        {
            Environment.SetEnvironmentVariable(key, _originalValues[key]);
        }
    }

    private static readonly string[] ManagedVars =
    {
        "INTEGRATION_HORIZON_URL",
        "INTEGRATION_HORIZON_TOKEN",
        "INTEGRATION_FRIENDBOT_URL",
        "INTEGRATION_STELLAR_RPC_URL",
        "INTEGRATION_STELLAR_RPC_TOKEN",
    };

    private readonly Dictionary<string, string?> _originalValues = new();

    [Test]
    public void HorizonUrl_WhenEnvVarUnset_ReturnsPublicTestnetDefault()
    {
        TestnetConfig.HorizonUrl.Should().Be("https://horizon-testnet.stellar.org");
    }

    [Test]
    public void HorizonUrl_WhenEnvVarSet_ReturnsConfiguredValue()
    {
        Environment.SetEnvironmentVariable("INTEGRATION_HORIZON_URL", "https://horizon.example.com");
        TestnetConfig.HorizonUrl.Should().Be("https://horizon.example.com");
    }

    [Test]
    public void HorizonUrl_WhenEnvVarWhitespace_ReturnsPublicTestnetDefault()
    {
        Environment.SetEnvironmentVariable("INTEGRATION_HORIZON_URL", "   ");
        TestnetConfig.HorizonUrl.Should().Be("https://horizon-testnet.stellar.org");
    }

    [Test]
    public void HorizonToken_WhenEnvVarUnset_ReturnsNull()
    {
        TestnetConfig.HorizonToken.Should().BeNull();
    }

    [Test]
    public void HorizonToken_WhenEnvVarSet_ReturnsConfiguredValue()
    {
        Environment.SetEnvironmentVariable("INTEGRATION_HORIZON_TOKEN", "secret-token");
        TestnetConfig.HorizonToken.Should().Be("secret-token");
    }

    [Test]
    public void FriendbotUrl_WhenEnvVarUnset_ReturnsPublicTestnetDefault()
    {
        TestnetConfig.FriendbotUrl.Should().Be("https://horizon-testnet.stellar.org");
    }

    [Test]
    public void FriendbotUrl_WhenEnvVarSet_ReturnsConfiguredValue()
    {
        Environment.SetEnvironmentVariable("INTEGRATION_FRIENDBOT_URL", "https://friendbot.example.com");
        TestnetConfig.FriendbotUrl.Should().Be("https://friendbot.example.com");
    }

    [Test]
    public void StellarRpcUrl_WhenEnvVarUnset_ReturnsPublicTestnetDefault()
    {
        TestnetConfig.StellarRpcUrl.Should().Be("https://soroban-testnet.stellar.org");
    }

    [Test]
    public void StellarRpcUrl_WhenEnvVarSet_ReturnsConfiguredValue()
    {
        Environment.SetEnvironmentVariable("INTEGRATION_STELLAR_RPC_URL", "https://rpc.example.com");
        TestnetConfig.StellarRpcUrl.Should().Be("https://rpc.example.com");
    }

    [Test]
    public void StellarRpcToken_WhenEnvVarUnset_ReturnsNull()
    {
        TestnetConfig.StellarRpcToken.Should().BeNull();
    }

    [Test]
    public void StellarRpcToken_WhenEnvVarSet_ReturnsConfiguredValue()
    {
        Environment.SetEnvironmentVariable("INTEGRATION_STELLAR_RPC_TOKEN", "rpc-token");
        TestnetConfig.StellarRpcToken.Should().Be("rpc-token");
    }

    [Test]
    public void NetworkPassphrase_ReturnsSdkTestnetConstant()
    {
        TestnetConfig.NetworkPassphrase.Should().Be(Network.TestnetPassphrase);
    }
}