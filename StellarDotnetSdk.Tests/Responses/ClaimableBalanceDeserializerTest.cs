using System;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Predicates;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
///     Unit tests for deserializing claimable balance responses from JSON.
/// </summary>
[TestClass]
public class ClaimableBalanceDeserializerTest
{
    /// <summary>
    ///     Verifies that ClaimableBalanceResponse can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClaimableBalanceJson_ReturnsDeserializedClaimableBalance()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("claimableBalance.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var claimableBalance = JsonSerializer.Deserialize<ClaimableBalanceResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(claimableBalance);
        AssertTestData(claimableBalance);
    }

    /// <summary>
    ///     Verifies that ClaimableBalanceResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithClaimableBalance_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("claimableBalance.json");
        var json = File.ReadAllText(jsonPath);
        var claimableBalance = JsonSerializer.Deserialize<ClaimableBalanceResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(claimableBalance);
        var back = JsonSerializer.Deserialize<ClaimableBalanceResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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
        Assert.AreEqual(new DateTimeOffset(2025, 8, 18, 13, 2, 39, TimeSpan.Zero), claimableBalance.LastModifiedTime);
        Assert.AreEqual("65909-000000009832889118c5fcf2cfb3c082e079520c516300b5276b839cb934cf88ebc9244a",
            claimableBalance.PagingToken);

        Assert.AreEqual(true, claimableBalance.Flags.ClawbackEnabled);

        Assert.AreEqual(2, claimableBalance.Claimants.Length);
        var claimant1 = claimableBalance.Claimants[0];
        Assert.AreEqual("GARAAT5FYX52DGIETDXV5IEM7ZX3S645DCZ67ZLUNKBSNSLYL3UQKNQ6", claimant1.Destination);
        Assert.IsInstanceOfType(claimant1.Predicate, typeof(PredicateOr));
        var orPredicate = (PredicateOr)claimant1.Predicate;
        Assert.IsInstanceOfType(orPredicate.Left, typeof(PredicateUnconditional));
        Assert.IsInstanceOfType(orPredicate.Right, typeof(PredicateAnd));
        var andPredicate = (PredicateAnd)orPredicate.Right;
        Assert.IsInstanceOfType(andPredicate.Left, typeof(PredicateBeforeAbsoluteTime));
        Assert.IsInstanceOfType(andPredicate.Right, typeof(PredicateUnconditional));

        var absTimePredicate = (PredicateBeforeAbsoluteTime)andPredicate.Left;
        Assert.AreEqual("2032-04-06T00:26:36Z", absTimePredicate.AbsBefore);
        Assert.AreEqual(1964823996, absTimePredicate.AbsBeforeEpoch);

        var claimant2 = claimableBalance.Claimants[1];
        Assert.AreEqual("GCQ7BPXWUYUURVJMCCZDCQJOXPEW5HCDYEZD337GDGGOTBTW3N66PDHY", claimant2.Destination);
        Assert.IsInstanceOfType(claimant2.Predicate, typeof(PredicateUnconditional));

        Assert.IsNotNull(claimableBalance.Links);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/claimable_balances/000000009832889118c5fcf2cfb3c082e079520c516300b5276b839cb934cf88ebc9244a",
            claimableBalance.Links.Self.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/claimable_balances/000000009832889118c5fcf2cfb3c082e079520c516300b5276b839cb934cf88ebc9244a/operations{?cursor,limit,order}",
            claimableBalance.Links.Operations.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/claimable_balances/000000009832889118c5fcf2cfb3c082e079520c516300b5276b839cb934cf88ebc9244a/transactions{?cursor,limit,order}",
            claimableBalance.Links.Transactions.Href);
    }
}