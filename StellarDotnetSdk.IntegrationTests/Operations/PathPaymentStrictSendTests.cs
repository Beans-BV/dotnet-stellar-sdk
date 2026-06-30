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
public class PathPaymentStrictSendTests : OperationsTestBase
{
    [Test]
    public async Task PathPaymentStrictSend_DeliversDestinationAsset()
    {
        var market = await SetUpXlmToAssetMarketAsync("PATH", "1000", "1");
        var destination = await CreateFundedAccountAsync();
        await SubmitAsync(destination, new ChangeTrustOperation(market.Asset, "1000000"));
        var sender = await CreateFundedAccountAsync();

        // Send exactly 10 XLM; require at least 9 PATH delivered (1:1 market, leaves margin).
        await SubmitAsync(sender, new PathPaymentStrictSendOperation(
            new AssetTypeNative(), "10", destination, market.Asset, "9", null));

        var account = await LoadAccountAsync(destination);
        var delivered = account.Balances.Single(b => b.AssetCode == "PATH");
        decimal.Parse(delivered.BalanceString, CultureInfo.InvariantCulture).Should().BeGreaterThanOrEqualTo(9m);
    }
}