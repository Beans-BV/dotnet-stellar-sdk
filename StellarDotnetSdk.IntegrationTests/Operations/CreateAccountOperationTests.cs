using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.IntegrationTests.Operations;

[TestFixture]
[CancelAfter(60_000)]
public class CreateAccountOperationTests : IntegrationTestBase
{
    [Test]
    public async Task CreateAccountOperation_WithStartingBalance_CreatesAccountOnChain()
    {
        var funder = await CreateFundedAccountAsync();
        var newAccount = KeyPair.Random();

        var funderAccount = await Server.Accounts.Account(funder.AccountId);
        var operation = new CreateAccountOperation(newAccount, "10");
        var tx = new TransactionBuilder(funderAccount)
            .AddOperation(operation)
            .Build();
        tx.Sign(funder);

        var response = await Server.SubmitTransaction(tx);
        response.Should().NotBeNull();
        response!.IsSuccess.Should().BeTrue();

        var created = await LoadAccountAsync(newAccount);
        created.AccountId.Should().Be(newAccount.AccountId);
        var nativeBalance = created.Balances.Single(b => b.AssetType == "native");
        decimal.Parse(nativeBalance.BalanceString, CultureInfo.InvariantCulture)
            .Should().Be(10m);
    }
}