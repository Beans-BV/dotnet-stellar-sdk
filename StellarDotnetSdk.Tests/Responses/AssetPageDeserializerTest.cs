using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class AssetPageDeserializerTest
{
    [TestMethod]
    public void TestDeserializeAssetPage()
    {
        var jsonPath = Utils.GetTestDataPath("assetPage.json");
        var json = File.ReadAllText(jsonPath);
        var assetsPage = JsonSingleton.GetInstance<Page<AssetResponse>>(json);
        Assert.IsNotNull(assetsPage);
        AssertTestData(assetsPage);
    }

    [TestMethod]
    public void TestSerializeDeserializeAssetPage()
    {
        var jsonPath = Utils.GetTestDataPath("assetPage.json");
        var json = File.ReadAllText(jsonPath);
        var assetsPage = JsonConvert.DeserializeObject<Page<AssetResponse>>(json);
        var serialized = JsonConvert.SerializeObject(assetsPage);
        var back = JsonConvert.DeserializeObject<Page<AssetResponse>>(serialized);
        Assert.IsNotNull(back);
        AssertTestData(back);
    }

    [TestMethod]
    [Obsolete]
    public void TestAssetResponseFlagDefaultsToNotImmutable()
    {
        var assetResponseFlags = new AssetResponse.AssetResponseFlags
        {
            AuthRequired = true,
            AuthRevocable = true,
            AuthImmutable = false
        };
        Assert.IsTrue(assetResponseFlags.AuthRequired);
        Assert.IsTrue(assetResponseFlags.AuthRevocable);
        Assert.IsFalse(assetResponseFlags.AuthImmutable);
    }

    public static void AssertTestData(Page<AssetResponse> assetsPage)
    {
        Assert.AreEqual("https://horizon-testnet.stellar.org/assets?cursor=&limit=200&order=desc",
            assetsPage.Links.Self.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/assets?cursor=XYZ_GBHLV3AOWIUIHECHYPO5YAVCFJVJS3EKVH5F744VJBIQQQ6NZCFBJJEL_credit_alphanum4&limit=200&order=desc",
            assetsPage.Links.Next.Href);

        Assert.AreEqual("credit_alphanum4", assetsPage.Records[0].AssetType);
        Assert.AreEqual("ZZZ", assetsPage.Records[0].AssetCode);
        Assert.AreEqual("GCWMJP3GFA2V3M2GSTJUC7H3NM27XG6GDGWJZVM3S536JWYIS6BIWS35", assetsPage.Records[0].AssetIssuer);
        Assert.AreEqual("ZZZ_GCWMJP3GFA2V3M2GSTJUC7H3NM27XG6GDGWJZVM3S536JWYIS6BIWS35_credit_alphanum4",
            assetsPage.Records[0].PagingToken);
        Assert.AreEqual(1, assetsPage.Records[0].Accounts.Authorized);
        Assert.AreEqual(0, assetsPage.Records[0].Accounts.AuthorizedToMaintainLiabilities);
        Assert.AreEqual(0, assetsPage.Records[0].Accounts.Unauthorized);
        Assert.AreEqual("1200000000.0000000", assetsPage.Records[0].Balances.Authorized);
        Assert.AreEqual("0.0000000", assetsPage.Records[0].Balances.AuthorizedToMaintainLiabilities);
        Assert.AreEqual("0.0000000", assetsPage.Records[0].Balances.Unauthorized);
        Assert.AreEqual(0, assetsPage.Records[0].NumClaimableBalances);
        Assert.AreEqual("0.0000000", assetsPage.Records[0].ClaimableBalancesAmount);
        Assert.AreEqual("1200000000.0000000", assetsPage.Records[0].Amount);
        Assert.AreEqual(1, assetsPage.Records[0].NumAccounts);
        Assert.AreEqual("", assetsPage.Records[0].Links.Toml.Href);
        Assert.AreEqual(false, assetsPage.Records[0].Flags.AuthRequired);
        Assert.AreEqual(false, assetsPage.Records[0].Flags.AuthRevocable);
        Assert.AreEqual(true, assetsPage.Records[0].Flags.AuthImmutable);

        assetsPage.Records[0].NumLiquidityPools
            .Should().Be(1);

        assetsPage.Records[0].LiquidityPoolsAmount
            .Should().Be("70000000.0000000");
    }
}