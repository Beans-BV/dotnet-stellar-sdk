using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Claimants;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class PredicateDeserializerTest
{
    [TestMethod]
    public void TestPredicateDeserialize()
    {
        const string json =
            "{\"and\":[{\"or\":[{\"rel_before\":\"12\"},{\"abs_before\":\"2020-08-26T11:15:39Z\"}]},{\"not\":{\"unconditional\":true}}]}";
        var predicate = JsonSerializer.Deserialize<Predicate>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(predicate);

        // Test polymorphic deserialization
        Assert.IsInstanceOfType(predicate, typeof(PredicateAnd));
        var andPredicate = (PredicateAnd)predicate;

        Assert.IsInstanceOfType(andPredicate.Left, typeof(PredicateOr));
        var orPredicate = (PredicateOr)andPredicate.Left;

        Assert.IsInstanceOfType(andPredicate.Right, typeof(PredicateNot));
        var notPredicate = (PredicateNot)andPredicate.Right;

        Assert.IsInstanceOfType(orPredicate.Left, typeof(PredicateBeforeRelativeTime));
        var relBefore = (PredicateBeforeRelativeTime)orPredicate.Left;
        Assert.AreEqual(12, relBefore.RelBefore);

        Assert.IsInstanceOfType(orPredicate.Right, typeof(PredicateBeforeAbsoluteTime));
        var absBefore = (PredicateBeforeAbsoluteTime)orPredicate.Right;
        Assert.AreEqual("2020-08-26T11:15:39Z", absBefore.AbsBefore);

        Assert.IsInstanceOfType(notPredicate.Inner, typeof(PredicateUnconditional));

        // Test conversion to ClaimPredicate
        var claimPredicate = predicate.ToClaimPredicate();
        Assert.IsInstanceOfType(claimPredicate, typeof(ClaimPredicateAnd));

        var claimAnd = (ClaimPredicateAnd)claimPredicate;
        Assert.IsInstanceOfType(claimAnd.LeftPredicate, typeof(ClaimPredicateOr));
        Assert.IsInstanceOfType(claimAnd.RightPredicate, typeof(ClaimPredicateNot));
    }

    [TestMethod]
    public void TestPredicateUnconditional()
    {
        const string json = "{\"unconditional\":true}";
        var predicate = JsonSerializer.Deserialize<Predicate>(json, JsonOptions.DefaultOptions);

        Assert.IsNotNull(predicate);
        Assert.IsInstanceOfType(predicate, typeof(PredicateUnconditional));

        var claimPredicate = predicate.ToClaimPredicate();
        Assert.IsInstanceOfType(claimPredicate, typeof(ClaimPredicateUnconditional));
    }

    [TestMethod]
    public void TestPredicateBeforeAbsoluteTimeWithEpoch()
    {
        const string json = "{\"abs_before\":\"2020-08-26T11:15:39Z\",\"abs_before_epoch\":1598440539}";
        var predicate = JsonSerializer.Deserialize<Predicate>(json, JsonOptions.DefaultOptions);

        Assert.IsNotNull(predicate);
        Assert.IsInstanceOfType(predicate, typeof(PredicateBeforeAbsoluteTime));

        var absPredicate = (PredicateBeforeAbsoluteTime)predicate;
        Assert.AreEqual("2020-08-26T11:15:39Z", absPredicate.AbsBefore);
        Assert.AreEqual(1598440539, absPredicate.AbsBeforeEpoch);
    }

    [TestMethod]
    public void TestPredicateBeforeRelativeTime()
    {
        const string json = "{\"rel_before\":3600}";
        var predicate = JsonSerializer.Deserialize<Predicate>(json, JsonOptions.DefaultOptions);

        Assert.IsNotNull(predicate);
        Assert.IsInstanceOfType(predicate, typeof(PredicateBeforeRelativeTime));

        var relPredicate = (PredicateBeforeRelativeTime)predicate;
        Assert.AreEqual(3600, relPredicate.RelBefore);
    }

    [TestMethod]
    public void TestPredicateSerialize()
    {
        var predicate = new PredicateAnd(
            new PredicateOr(
                new PredicateBeforeRelativeTime(12),
                new PredicateBeforeAbsoluteTime("2020-08-26T11:15:39Z")
            ),
            new PredicateNot(new PredicateUnconditional())
        );

        // Serialize with explicit base type to ensure converter is used
        var json = JsonSerializer.Serialize<Predicate>(predicate, JsonOptions.DefaultOptions);
        Assert.IsNotNull(json);

        // Verify the JSON structure is correct
        Assert.IsTrue(json.Contains("\"and\""), $"JSON should contain 'and' property. Actual: {json}");

        // Deserialize back and verify
        var deserialized = JsonSerializer.Deserialize<Predicate>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(deserialized);
        Assert.IsInstanceOfType(deserialized, typeof(PredicateAnd));
    }

    [TestMethod]
    public void TestPredicateBeforeRelativeTimeWithStringValue()
    {
        // Horizon API may return numeric fields as strings
        const string json = "{\"rel_before\":\"3600\"}";
        var predicate = JsonSerializer.Deserialize<Predicate>(json, JsonOptions.DefaultOptions);

        Assert.IsNotNull(predicate);
        Assert.IsInstanceOfType(predicate, typeof(PredicateBeforeRelativeTime));

        var relPredicate = (PredicateBeforeRelativeTime)predicate;
        Assert.AreEqual(3600, relPredicate.RelBefore);
    }

    [TestMethod]
    public void TestPredicateBeforeAbsoluteTimeWithStringEpoch()
    {
        // Horizon API may return numeric fields as strings
        const string json = "{\"abs_before\":\"2020-08-26T11:15:39Z\",\"abs_before_epoch\":\"1598440539\"}";
        var predicate = JsonSerializer.Deserialize<Predicate>(json, JsonOptions.DefaultOptions);

        Assert.IsNotNull(predicate);
        Assert.IsInstanceOfType(predicate, typeof(PredicateBeforeAbsoluteTime));

        var absPredicate = (PredicateBeforeAbsoluteTime)predicate;
        Assert.AreEqual(1598440539, absPredicate.AbsBeforeEpoch);
    }
}