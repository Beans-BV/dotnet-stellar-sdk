using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.IntegrationTests.Infrastructure;

namespace StellarDotnetSdk.IntegrationTests.Friendbot;

[TestFixture]
[CancelAfter(60_000)]
public class FriendbotTests : IntegrationTestBase
{
    [Test]
    public async Task FundAccount_WithNewKeypair_CreatesAccountOnChain()
    {
        var keyPair = await CreateFundedAccountAsync();

        var account = await LoadAccountAsync(keyPair);

        account.AccountId.Should().Be(keyPair.AccountId);
        account.SequenceNumber.Should().BeGreaterThan(0);
        account.Balances.Should().NotBeEmpty();
        account.Balances.Any(b => b.AssetType == "native").Should().BeTrue();
    }
}