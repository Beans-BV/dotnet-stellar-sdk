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

        var account = await Server.Accounts.Account(keyPair.AccountId);

        account.AccountId.Should().Be(keyPair.AccountId);
        account.SequenceNumber.Should().BeGreaterThan(0);
        account.Balances.Should().Contain(b => b.AssetType == "native");
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