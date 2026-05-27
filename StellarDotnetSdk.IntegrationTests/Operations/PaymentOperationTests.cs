using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.IntegrationTests.Operations;

[TestFixture]
[CancelAfter(60_000)]
public class PaymentOperationTests : IntegrationTestBase
{
    [Test]
    public async Task PaymentOperation_WithNativeAsset_CreditsDestination()
    {
        var source = await CreateFundedAccountAsync();
        var destination = await CreateFundedAccountAsync();

        var destBefore = await LoadAccountAsync(destination);
        var destNativeBefore = decimal.Parse(
            destBefore.Balances.Single(b => b.AssetType == "native").BalanceString,
            CultureInfo.InvariantCulture);

        var sourceAccount = await Server.Accounts.Account(source.AccountId);
        var payment = new PaymentOperation(destination, new AssetTypeNative(), "5");
        var tx = new TransactionBuilder(sourceAccount)
            .AddOperation(payment)
            .Build();
        tx.Sign(source);

        var response = await Server.SubmitTransaction(tx);
        response.Should().NotBeNull();
        response!.IsSuccess.Should().BeTrue();

        var destAfter = await LoadAccountAsync(destination);
        var destNativeAfter = decimal.Parse(
            destAfter.Balances.Single(b => b.AssetType == "native").BalanceString,
            CultureInfo.InvariantCulture);

        (destNativeAfter - destNativeBefore).Should().Be(5m);
    }
}