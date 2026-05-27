using System;
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
        _originalHorizonUrl = Environment.GetEnvironmentVariable("INTEGRATION_HORIZON_URL");
        _originalSorobanRpcUrl = Environment.GetEnvironmentVariable("INTEGRATION_SOROBAN_RPC_URL");
        Environment.SetEnvironmentVariable("INTEGRATION_HORIZON_URL", null);
        Environment.SetEnvironmentVariable("INTEGRATION_SOROBAN_RPC_URL", null);
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("INTEGRATION_HORIZON_URL", _originalHorizonUrl);
        Environment.SetEnvironmentVariable("INTEGRATION_SOROBAN_RPC_URL", _originalSorobanRpcUrl);
    }

    private string? _originalHorizonUrl;
    private string? _originalSorobanRpcUrl;

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
    public void SorobanRpcUrl_WhenEnvVarUnset_ReturnsPublicTestnetDefault()
    {
        TestnetConfig.SorobanRpcUrl.Should().Be("https://soroban-testnet.stellar.org");
    }

    [Test]
    public void SorobanRpcUrl_WhenEnvVarSet_ReturnsConfiguredValue()
    {
        Environment.SetEnvironmentVariable("INTEGRATION_SOROBAN_RPC_URL", "https://soroban.example.com");
        TestnetConfig.SorobanRpcUrl.Should().Be("https://soroban.example.com");
    }

    [Test]
    public void NetworkPassphrase_ReturnsSdkTestnetConstant()
    {
        TestnetConfig.NetworkPassphrase.Should().Be(Network.TestnetPassphrase);
    }
}