using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Operations;

namespace StellarDotnetSdk.IntegrationTests.Horizon;

[TestFixture]
[CancelAfter(60_000)]
public class AccountsRequestBuilderTests : OperationsTestBase
{
    [Test]
    public async Task Account_ForFundedAccount_ReturnsState()
    {
        var keyPair = await CreateFundedAccountAsync();

        var account = await LoadAccountAsync(keyPair);

        account.AccountId.Should().Be(keyPair.AccountId);
        account.SequenceNumber.Should().BeGreaterThan(0);
        var native = account.Balances.Should().ContainSingle(b => b.AssetType == "native").Which;
        // Friendbot currently grants 10,000 XLM. The wide range tolerates faucet policy changes and
        // non-SDF faucets while still catching decimal/stroop scale regressions (a 10^7 factor lands
        // far outside it).
        decimal.Parse(native.BalanceString, CultureInfo.InvariantCulture).Should()
            .BeInRange(1_000m, 100_000m);
    }

    [Test]
    public async Task AccountData_ForStoredEntry_ReturnsDecodedValue()
    {
        var keyPair = await CreateFundedAccountAsync();
        await SubmitAsync(keyPair, new ManageDataOperation("favorite_color", "blue"));

        var data = await Server.Accounts.AccountData(keyPair.AccountId, "favorite_color");

        data.ValueDecoded.Should().Be("blue");
    }
}