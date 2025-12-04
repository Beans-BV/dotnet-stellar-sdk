using System;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;
using MuxedAccount = StellarDotnetSdk.Xdr.MuxedAccount;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class AccountDeserializerTest
{
    [TestMethod]
    public void TestDeserializeAccountResponse()
    {
        var jsonPath = Utils.GetTestDataPath("account.json");
        var json = File.ReadAllText(jsonPath);
        var account = JsonSerializer.Deserialize<AccountResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(account);
        AssertTestData(account);
    }

    [TestMethod]
    public void TestMuxedAccountException()
    {
        var account = new Account(new UnknownAccountId(), 128);

        var ex = Assert.ThrowsException<Exception>(() => account.KeyPair);
        Assert.AreEqual("Invalid Account MuxedAccount type", ex.Message);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountResponse()
    {
        var jsonPath = Utils.GetTestDataPath("account.json");
        var json = File.ReadAllText(jsonPath);
        var account = JsonSerializer.Deserialize<AccountResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(account);
        var back = JsonSerializer.Deserialize<AccountResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertTestData(back);
    }

    public static void AssertTestData(AccountResponse account)
    {
        Assert.AreEqual("GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST", account.PagingToken);
        Assert.AreEqual("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7", account.AccountId);
        Assert.AreEqual(6674379177987L, account.SequenceNumber);
        Assert.AreEqual(0, account.SubentryCount);
        Assert.AreEqual(1234L, account.SequenceUpdatedAtLedger);
        Assert.AreEqual(1755199978L, account.SequenceUpdatedAtTime);
        Assert.AreEqual("GAGRSA6QNQJN2OQYCBNQGMFLO4QLZFNEHIFXOMTQVSUTWVTWT66TOFSC", account.InflationDestination);
        Assert.AreEqual("stellar.org", account.HomeDomain);

        Assert.AreEqual(10, account.Thresholds.LowThreshold);
        Assert.AreEqual(20, account.Thresholds.MedThreshold);
        Assert.AreEqual(30, account.Thresholds.HighThreshold);

        Assert.AreEqual(false, account.Flags.AuthRequired);
        Assert.AreEqual(true, account.Flags.AuthRevocable);
        Assert.AreEqual(true, account.Flags.AuthImmutable);
        Assert.AreEqual(true, account.Flags.AuthClawback);

        Assert.AreEqual(3, account.Balances.Length);
        var balance1 = account.Balances[0];
        Assert.AreEqual("credit_alphanum4", balance1.AssetType);
        Assert.AreEqual("ABC", balance1.AssetCode);
        Assert.AreEqual("GCRA6COW27CY5MTKIA7POQ2326C5ABYCXODBN4TFF5VL4FMBRHOT3YHU", balance1.AssetIssuer);
        Assert.IsNotNull(balance1.Asset);
        var asset = (AssetTypeCreditAlphaNum)balance1.Asset;
        Assert.IsInstanceOfType(asset, typeof(AssetTypeCreditAlphaNum));
        Assert.AreEqual("ABC", asset.Code);
        Assert.AreEqual("GCRA6COW27CY5MTKIA7POQ2326C5ABYCXODBN4TFF5VL4FMBRHOT3YHU", asset.Issuer);

        Assert.AreEqual("1001.0000000", balance1.BalanceString);
        Assert.AreEqual("12000.4775807", balance1.Limit);
        Assert.AreEqual("100.1234567", balance1.BuyingLiabilities);
        Assert.AreEqual("100.7654321", balance1.SellingLiabilities);
        Assert.AreEqual("100.7654321", balance1.SellingLiabilities);
        Assert.AreEqual(false, balance1.IsAuthorized);
        Assert.AreEqual(true, balance1.IsAuthorizedToMaintainLiabilities);

        var balance2 = account.Balances[1];
        Assert.AreEqual("native", balance2.AssetType);
        Assert.IsInstanceOfType(balance2.Asset, typeof(AssetTypeNative));
        Assert.AreEqual("20.0000300", balance2.BalanceString);
        Assert.AreEqual("5.1234567", balance2.BuyingLiabilities);
        Assert.AreEqual("1.7654321", balance2.SellingLiabilities);
        Assert.IsNull(balance2.Limit);

        var balance3 = account.Balances[2];
        Assert.IsNull(balance3.Asset);
        Assert.AreEqual("liquidity_pool_shares", balance3.AssetType);
        Assert.AreEqual("500.0000400", balance3.BalanceString);
        Assert.AreEqual("922337203685.4775807", balance3.Limit);
        Assert.AreEqual(false, balance3.IsAuthorized);
        Assert.AreEqual(false, balance3.IsAuthorizedToMaintainLiabilities);
        Assert.AreEqual("1c80ecd9cc567ef5301683af3ca7c2deeba7d519275325549f22514076396469", balance3.LiquidityPoolId);

        Assert.AreEqual("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7", account.Signers[0].Key);
        Assert.AreEqual(0, account.Signers[0].Weight);
        Assert.AreEqual("ed25519_public_key", account.Signers[0].Type);
        Assert.AreEqual("GCR2KBCIU6KQXSQY5F5GZYC4WLNHCHCKW4NEGXNEZRYWLTNZIRJJY7D2", account.Signers[1].Key);
        Assert.AreEqual(1, account.Signers[1].Weight);
        Assert.AreEqual("ed25519_public_key", account.Signers[1].Type);

        Assert.AreEqual("VGVzdFZhbHVl", account.Data["TestKey"]);
        Assert.AreEqual(0, account.NumberSponsoring);
        Assert.AreEqual(0, account.NumberSponsored);
        Assert.AreEqual(1558L, account.LastModifiedLedger);
        Assert.AreEqual(new DateTimeOffset(2025, 8, 18, 13, 02, 39, TimeSpan.Zero), account.LastModifiedTime);

        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST",
            account.Links.Self.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST/transactions{?cursor,limit,order}",
            account.Links.Transactions.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST/operations{?cursor,limit,order}",
            account.Links.Operations.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST/payments{?cursor,limit,order}",
            account.Links.Payments.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST/effects{?cursor,limit,order}",
            account.Links.Effects.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST/offers{?cursor,limit,order}",
            account.Links.Offers.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST/trades{?cursor,limit,order}",
            account.Links.Trades.Href);
    }

    private class UnknownAccountId : IAccountId
    {
        public MuxedAccount MuxedAccount => throw new NotImplementedException();

        public byte[] PublicKey => throw new NotImplementedException();

        public string Address => throw new NotImplementedException();

        public string AccountId => throw new NotImplementedException();

        public bool IsMuxedAccount => throw new NotImplementedException();

        public KeyPair SigningKey => throw new NotImplementedException();
    }
}