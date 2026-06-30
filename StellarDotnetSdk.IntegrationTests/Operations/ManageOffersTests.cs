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
[CancelAfter(90_000)]
public class ManageOffersTests : OperationsTestBase
{
    [Test]
    public async Task ManageSellOffer_CreateUpdateDelete()
    {
        var issuer = await CreateFundedAccountAsync();
        var seller = await CreateFundedAccountAsync();
        var asset = Asset.CreateNonNativeAsset("SELL", issuer.AccountId);
        var native = new AssetTypeNative();

        // Seller trusts and receives the asset, then offers to sell it for XLM.
        await SubmitAsync(seller, new ChangeTrustOperation(asset, "1000"));
        await SubmitAsync(issuer, new PaymentOperation(seller, asset, "500"));
        await SubmitAsync(seller, new ManageSellOfferOperation(asset, native, "100", "0.5", 0));

        var afterCreate = await Server.Offers.ForAccount(seller.AccountId).Execute();
        afterCreate.Records.Should().HaveCount(1);
        var offer = afterCreate.Records[0];
        offer.Selling.Should().Be(asset);
        decimal.Parse(offer.Amount, CultureInfo.InvariantCulture).Should().Be(100m);
        var offerId = long.Parse(offer.Id, CultureInfo.InvariantCulture);

        // Update: same offerId, new amount.
        await SubmitAsync(seller, new ManageSellOfferOperation(asset, native, "40", "0.5", offerId));
        var afterUpdate = await Server.Offers.ForAccount(seller.AccountId).Execute();
        var updated = afterUpdate.Records.Single();
        updated.Id.Should().Be(offer.Id, "the same offer should be mutated, not replaced");
        decimal.Parse(updated.Amount, CultureInfo.InvariantCulture).Should().Be(40m);

        // Delete: amount "0".
        await SubmitAsync(seller, new ManageSellOfferOperation(asset, native, "0", "0.5", offerId));
        var afterDelete = await Server.Offers.ForAccount(seller.AccountId).Execute();
        afterDelete.Records.Should().BeEmpty();
    }

    [Test]
    public async Task ManageBuyOffer_CreatesRestingOffer()
    {
        var issuer = await CreateFundedAccountAsync();
        var buyer = await CreateFundedAccountAsync();
        var asset = Asset.CreateNonNativeAsset("BUY", issuer.AccountId);
        var native = new AssetTypeNative();

        // Buyer offers XLM to buy the asset; nobody sells it, so the offer rests.
        await SubmitAsync(buyer, new ChangeTrustOperation(asset, "1000"));
        await SubmitAsync(buyer, new ManageBuyOfferOperation(native, asset, "10", "2", 0));

        var offers = await Server.Offers.ForAccount(buyer.AccountId).Execute();
        offers.Records.Should().HaveCount(1);
        offers.Records[0].Buying.Should().Be(asset);
    }
}