using System.Threading.Tasks;
using FluentAssertions;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.IntegrationTests.Infrastructure;

/// <summary>
///     Base for classic-operation integration tests. Adds a submit-and-assert helper on top of
///     <see cref="IntegrationTestBase" /> so fixtures don't repeat the
///     load-account → build → sign → submit → assert cycle, plus a shared orderbook scaffold
///     used by the path-payment fixtures.
/// </summary>
public abstract class OperationsTestBase : IntegrationTestBase
{
    /// <summary>
    ///     Loads <paramref name="source" />'s current account, builds a transaction from
    ///     <paramref name="operations" />, signs it with <paramref name="source" />, submits it,
    ///     asserts success, and returns the response.
    /// </summary>
    protected async Task<SubmitTransactionResponse> SubmitAsync(KeyPair source, params Operation[] operations)
    {
        var sourceAccount = await Server.Accounts.Account(source.AccountId);
        var builder = new TransactionBuilder(sourceAccount);
        foreach (var operation in operations)
        {
            builder.AddOperation(operation);
        }

        var tx = builder.Build();
        tx.Sign(source);

        var response = await Server.SubmitTransaction(tx);
        response.Should().NotBeNull();
        response!.IsSuccess.Should().BeTrue("transaction should be accepted by Testnet");
        return response;
    }

    /// <summary>
    ///     Provisions an XLM→asset path: a fresh issuer, and a market maker that trusts the asset,
    ///     receives <paramref name="offerAmount" /> of it, and rests a sell offer (selling the asset
    ///     for native XLM at <paramref name="price" /> XLM per unit). A sender paying XLM can then
    ///     deliver the asset to any destination that trusts it.
    /// </summary>
    protected async Task<PathMarket> SetUpXlmToAssetMarketAsync(string assetCode, string offerAmount, string price)
    {
        var issuer = await CreateFundedAccountAsync();
        var marketMaker = await CreateFundedAccountAsync();
        var asset = Asset.CreateNonNativeAsset(assetCode, issuer.AccountId);

        await SubmitAsync(marketMaker, new ChangeTrustOperation(asset, "1000000"));
        await SubmitAsync(issuer, new PaymentOperation(marketMaker, asset, offerAmount));
        await SubmitAsync(marketMaker,
            new ManageSellOfferOperation(asset, new AssetTypeNative(), offerAmount, price, 0));

        return new PathMarket(issuer, marketMaker, asset);
    }

    /// <summary>A funded asset issuer + a market maker resting a sell offer (asset → XLM).</summary>
    protected sealed record PathMarket(
        KeyPair Issuer,
        KeyPair MarketMaker,
        Asset Asset);
}