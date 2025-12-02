using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class ClaimableBalanceDeserializerTest
{
    [TestMethod]
    public void TestDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalance.json");
        var json = File.ReadAllText(jsonPath);
        var claimableBalance = JsonSerializer.Deserialize<ClaimableBalanceResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(claimableBalance);
        AssertTestData(claimableBalance);
    }

    [TestMethod]
    public void TestSerializeDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalance.json");
        var json = File.ReadAllText(jsonPath);
        var claimableBalance = JsonSerializer.Deserialize<ClaimableBalanceResponse>(json, JsonOptions.DefaultOptions);

        var serialized = JsonSerializer.Serialize(claimableBalance);
        var back = JsonSerializer.Deserialize<ClaimableBalanceResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        Assert.IsNotNull(claimableBalance);
        Assert.AreEqual(claimableBalance.LastModifiedLedger, back.LastModifiedLedger);
        Assert.AreEqual(claimableBalance.Asset, back.Asset);
        Assert.AreEqual(claimableBalance.Sponsor, back.Sponsor);
    }

    public static void AssertTestData(ClaimableBalanceResponse claimableBalance)
    {
        Assert.AreEqual("000000009832889118c5fcf2cfb3c082e079520c516300b5276b839cb934cf88ebc9244a",
            claimableBalance.Id);
        Assert.AreEqual("govICE:GDERZDEWIYBPWFQLG7GV5BWC4BXSD5KCQ734D42P72IG5COAYIFB2DTB", claimableBalance.Asset);
        Assert.AreEqual("16.6666667", claimableBalance.Amount);
        Assert.AreEqual("GDERZDEWIYBPWFQLG7GV5BWC4BXSD5KCQ734D42P72IG5COAYIFB2DTB", claimableBalance.Sponsor);
        Assert.AreEqual(65909, claimableBalance.LastModifiedLedger);
        Assert.AreEqual("2025-08-18T13:02:39Z", claimableBalance.LastModifiedTime);
        Assert.AreEqual("65909-000000009832889118c5fcf2cfb3c082e079520c516300b5276b839cb934cf88ebc9244a",
            claimableBalance.PagingToken);

        Assert.AreEqual(true, claimableBalance.Flags.ClawbackEnabled);
        
        Assert.AreEqual(2, claimableBalance.Claimants.Length);
        var claimant1 = claimableBalance.Claimants[0];
        Assert.AreEqual("GARAAT5FYX52DGIETDXV5IEM7ZX3S645DCZ67ZLUNKBSNSLYL3UQKNQ6", claimant1.Destination);
        Assert.IsNotNull(claimant1.Predicate.Not);
        Assert.AreEqual("2025-08-18T12:55:42Z", claimant1.Predicate.Not.AbsBefore);
        Assert.AreEqual(1755521742, claimant1.Predicate.Not.AbsBeforeEpoch);
        
        var claimant2 = claimableBalance.Claimants[1];
        Assert.AreEqual("GCQ7BPXWUYUURVJMCCZDCQJOXPEW5HCDYEZD337GDGGOTBTW3N66PDHY", claimant2.Destination);
        Assert.AreEqual(true, claimant2.Predicate.Unconditional);
        
        Assert.IsNotNull(claimableBalance.Links);
        Assert.AreEqual("https://horizon-testnet.stellar.org/claimable_balances/000000009832889118c5fcf2cfb3c082e079520c516300b5276b839cb934cf88ebc9244a", claimableBalance.Links.Self.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/claimable_balances/000000009832889118c5fcf2cfb3c082e079520c516300b5276b839cb934cf88ebc9244a/operations{?cursor,limit,order}", claimableBalance.Links.Operations.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/claimable_balances/000000009832889118c5fcf2cfb3c082e079520c516300b5276b839cb934cf88ebc9244a/transactions{?cursor,limit,order}", claimableBalance.Links.Transactions.Href);
    }
}