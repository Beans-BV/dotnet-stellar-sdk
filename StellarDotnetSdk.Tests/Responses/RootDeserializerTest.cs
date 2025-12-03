using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class RootDeserializerTest
{
    [TestMethod]
    public void TestDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("root.json");
        var json = File.ReadAllText(jsonPath);
        var root = JsonSerializer.Deserialize<RootResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(root);
        AssertTestData(root);
    }

    [TestMethod]
    public void TestSerializeDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("root.json");
        var json = File.ReadAllText(jsonPath);
        var root = JsonSerializer.Deserialize<RootResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(root);
        var back = JsonSerializer.Deserialize<RootResponse>(serialized, JsonOptions.DefaultOptions);

        Assert.IsNotNull(back);
        AssertTestData(back);
    }


    private static void AssertTestData(RootResponse root)
    {
        Assert.AreEqual("24.0.0-479385ffcbf959dad6463bb17917766f5cb4d43f", root.HorizonVersion);
        Assert.AreEqual("stellar-core 24.1.0 (5a7035d49201b88db95e024b343fb866c2185043)", root.StellarCoreVersion);
        Assert.AreEqual(18369116L, root.HistoryLatestLedger);
        Assert.AreEqual("2025-12-01T04:30:48Z", root.HistoryLatestLedgerClosedAt);
        Assert.AreEqual(53789041L, root.HistoryElderLedger);
        Assert.AreEqual(18369117L, root.CoreLatestLedger);
        Assert.AreEqual("Public Global Stellar Network ; September 2015", root.NetworkPassphrase);
        Assert.AreEqual(24, root.CurrentProtocolVersion);
        Assert.AreEqual(24, root.CoreSupportedProtocolVersion);
        Assert.AreEqual(24, root.SupportedProtocolVersion);

        // Assert links
        Assert.IsNotNull(root.Links);
        Assert.AreEqual("https://horizon-testnet.stellar.org/accounts/{account_id}", root.Links.Account.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts{?signer,sponsor,asset,liquidity_pool,cursor,limit,order}",
            root.Links.Accounts.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/accounts/{account_id}/transactions{?cursor,limit,order}",
            root.Links.AccountTransactions.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/assets{?asset_code,asset_issuer,cursor,limit,order}",
            root.Links.Assets.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/claimable_balances{?asset,sponsor,claimant,cursor,limit,order}",
            root.Links.ClaimableBalances.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/effects{?cursor,limit,order}", root.Links.Effects.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/fee_stats", root.Links.FeeStats.Href);
        Assert.IsNotNull(root.Links.Friendbot);
        Assert.AreEqual("https://friendbot.stellar.org/{?addr}", root.Links.Friendbot.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers/{sequence}", root.Links.Ledger.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers{?cursor,limit,order}", root.Links.Ledgers.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/liquidity_pools{?reserves,account,cursor,limit,order}",
            root.Links.LiquidityPools.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/offers/{offer_id}", root.Links.Offer.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/offers{?selling,buying,seller,sponsor,cursor,limit,order}",
            root.Links.Offers.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/operations/{id}", root.Links.Operation.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/operations{?cursor,limit,order,include_failed}",
            root.Links.Operations.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/order_book{?selling_asset_type,selling_asset_code,selling_asset_issuer,buying_asset_type,buying_asset_code,buying_asset_issuer,limit}",
            root.Links.OrderBook.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/payments{?cursor,limit,order,include_failed}",
            root.Links.Payments.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/", root.Links.Self.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/paths/strict-receive{?source_assets,source_account,destination_account,destination_asset_type,destination_asset_issuer,destination_asset_code,destination_amount}",
            root.Links.StrictReceivePaths.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/paths/strict-send{?destination_account,destination_assets,source_asset_type,source_asset_issuer,source_asset_code,source_amount}",
            root.Links.StrictSendPaths.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/trade_aggregations?base_asset_type={base_asset_type}&base_asset_code={base_asset_code}&base_asset_issuer={base_asset_issuer}&counter_asset_type={counter_asset_type}&counter_asset_code={counter_asset_code}&counter_asset_issuer={counter_asset_issuer}",
            root.Links.TradeAggregations.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/trades?base_asset_type={base_asset_type}&base_asset_code={base_asset_code}&base_asset_issuer={base_asset_issuer}&counter_asset_type={counter_asset_type}&counter_asset_code={counter_asset_code}&counter_asset_issuer={counter_asset_issuer}",
            root.Links.Trades.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/transactions/{hash}", root.Links.Transaction.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/transactions{?cursor,limit,order}",
            root.Links.Transactions.Href);
    }
}