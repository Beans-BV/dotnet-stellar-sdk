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
public class PaymentOperationTests : OperationsTestBase
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

    [Test]
    public async Task PaymentOperation_WithNonNativeAsset_CreditsDestination()
    {
        var issuer = await CreateFundedAccountAsync();
        var recipient = await CreateFundedAccountAsync();
        var asset = Asset.CreateNonNativeAsset("USD", issuer.AccountId);

        // The recipient must trust the asset before the issuer can pay it.
        await SubmitAsync(recipient, new ChangeTrustOperation(asset, "1000"));
        await SubmitAsync(issuer, new PaymentOperation(recipient, asset, "250"));

        var account = await LoadAccountAsync(recipient);
        var balance = account.Balances.Single(b => b.AssetCode == "USD" && b.AssetIssuer == issuer.AccountId);
        decimal.Parse(balance.BalanceString, CultureInfo.InvariantCulture).Should().Be(250m);
    }
}