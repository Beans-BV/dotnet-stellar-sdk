using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Operations;

namespace StellarDotnetSdk.IntegrationTests.Operations;

[TestFixture]
[CancelAfter(120_000)]
public class PathPaymentStrictReceiveTests : OperationsTestBase
{
    [Test]
    public async Task PathPaymentStrictReceive_DeliversExactDestinationAmount()
    {
        var asset = await SetUpXlmToAssetMarketAsync();
        // The destination and sender are independent; fund them concurrently.
        var accounts = await Task.WhenAll(CreateFundedAccountAsync(), CreateFundedAccountAsync());
        var destination = accounts[0];
        var sender = accounts[1];
        await SubmitAsync(destination, new ChangeTrustOperation(asset, "1000000"));

        // Require exactly 10 PATH delivered; allow up to 20 XLM spent.
        await SubmitAsync(sender, new PathPaymentStrictReceiveOperation(
            new AssetTypeNative(), "20", destination, asset, "10", null));

        var account = await LoadAccountAsync(destination);
        var delivered = account.Balances.Single(b => b.AssetCode == "PATH");
        decimal.Parse(delivered.BalanceString, CultureInfo.InvariantCulture).Should().Be(10m);
    }
}