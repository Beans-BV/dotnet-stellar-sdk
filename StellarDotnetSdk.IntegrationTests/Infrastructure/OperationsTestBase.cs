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
        var sourceAccount = await LoadAccountAsync(source);
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
    ///     Provisions an XLM→asset market on a fresh issuer: a market maker trusts the asset, receives
    ///     1000 of it, and rests a sell offer (selling the asset for native XLM at 1:1). A sender paying
    ///     XLM can then deliver the asset to any destination that trusts it. Returns the issued asset.
    /// </summary>
    protected async Task<Asset> SetUpXlmToAssetMarketAsync()
    {
        // The issuer and market maker are independent; fund them concurrently to halve setup latency.
        var accounts = await Task.WhenAll(CreateFundedAccountAsync(), CreateFundedAccountAsync());
        var issuer = accounts[0];
        var marketMaker = accounts[1];
        var asset = Asset.CreateNonNativeAsset("PATH", issuer.AccountId);

        await SubmitAsync(marketMaker, new ChangeTrustOperation(asset, "1000000"));
        await SubmitAsync(issuer, new PaymentOperation(marketMaker, asset, "1000"));
        await SubmitAsync(marketMaker,
            new ManageSellOfferOperation(asset, new AssetTypeNative(), "1000", "1", 0));

        return asset;
    }
}