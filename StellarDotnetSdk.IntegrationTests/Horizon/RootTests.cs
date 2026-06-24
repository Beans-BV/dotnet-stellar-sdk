using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.IntegrationTests.Infrastructure;

namespace StellarDotnetSdk.IntegrationTests.Horizon;

[TestFixture]
[CancelAfter(30_000)]
public class RootTests : IntegrationTestBase
{
    [Test]
    public async Task RootAsync_OnTestnet_ReturnsPassphraseAndSupportedProtocol()
    {
        var root = await Server.RootAsync();

        root.Should().NotBeNull();
        root!.NetworkPassphrase.Should().Be(TestnetConfig.NetworkPassphrase);
        root.CurrentProtocolVersion.Should().BeGreaterThanOrEqualTo(20);
        root.HorizonVersion.Should().NotBeNullOrWhiteSpace();
    }
}