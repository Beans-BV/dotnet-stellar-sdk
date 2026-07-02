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
[CancelAfter(60_000)]
public class ChangeTrustOperationTests : OperationsTestBase
{
    [Test]
    public async Task ChangeTrust_WithLimit_CreatesTrustline()
    {
        var issuer = await CreateFundedAccountAsync();
        var holder = await CreateFundedAccountAsync();
        var asset = Asset.CreateNonNativeAsset("TEST", issuer.AccountId);

        await SubmitAsync(holder, new ChangeTrustOperation(asset, "1000"));

        var account = await LoadAccountAsync(holder);
        var trustline = account.Balances.SingleOrDefault(b => b.AssetCode == "TEST");
        trustline.Should().NotBeNull("the TEST trustline should exist");
        trustline!.AssetIssuer.Should().Be(issuer.AccountId);
        decimal.Parse(trustline.Limit!, CultureInfo.InvariantCulture).Should().Be(1000m);
    }

    [Test]
    public async Task ChangeTrust_WithZeroLimit_RemovesTrustline()
    {
        var issuer = await CreateFundedAccountAsync();
        var holder = await CreateFundedAccountAsync();
        var asset = Asset.CreateNonNativeAsset("TEST", issuer.AccountId);

        await SubmitAsync(holder, new ChangeTrustOperation(asset, "1000"));
        await SubmitAsync(holder, new ChangeTrustOperation(asset, "0"));

        var account = await LoadAccountAsync(holder);
        account.Balances.Any(b => b.AssetCode == "TEST").Should().BeFalse("the trustline should be removed");
    }
}