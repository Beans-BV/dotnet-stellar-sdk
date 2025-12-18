using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Claimants;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Predicates;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
///     Unit tests for deserializing predicate responses from JSON.
/// </summary>
[TestClass]
public class PredicateDeserializerTest
{
    /// <summary>
    ///     Verifies that complex Predicate with nested And, Or, and Not can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithComplexPredicateJson_ReturnsDeserializedPredicate()
    {
        // Arrange
        const string json =
            "{\"and\":[{\"or\":[{\"rel_before\":\"12\"},{\"abs_before\":\"2020-08-26T11:15:39Z\"}]},{\"not\":{\"unconditional\":true}}]}";

        // Act
        var predicate = JsonSerializer.Deserialize<Predicate>(json, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that PredicateUnconditional can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithUnconditionalPredicateJson_ReturnsPredicateUnconditional()
    {
        // Arrange
        const string json = "{\"unconditional\":true}";

        // Act
        var predicate = JsonSerializer.Deserialize<Predicate>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(predicate);
        Assert.IsInstanceOfType(predicate, typeof(PredicateUnconditional));

        var claimPredicate = predicate.ToClaimPredicate();
        Assert.IsInstanceOfType(claimPredicate, typeof(ClaimPredicateUnconditional));
    }

    /// <summary>
    ///     Verifies that PredicateBeforeAbsoluteTime with epoch can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAbsoluteTimePredicateJson_ReturnsPredicateBeforeAbsoluteTime()
    {
        // Arrange
        const string json = "{\"abs_before\":\"2020-08-26T11:15:39Z\",\"abs_before_epoch\":1598440539}";

        // Act
        var predicate = JsonSerializer.Deserialize<Predicate>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(predicate);
        Assert.IsInstanceOfType(predicate, typeof(PredicateBeforeAbsoluteTime));

        var absPredicate = (PredicateBeforeAbsoluteTime)predicate;
        Assert.AreEqual("2020-08-26T11:15:39Z", absPredicate.AbsBefore);
        Assert.AreEqual(1598440539, absPredicate.AbsBeforeEpoch);
    }

    /// <summary>
    ///     Verifies that PredicateBeforeRelativeTime can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithRelativeTimePredicateJson_ReturnsPredicateBeforeRelativeTime()
    {
        // Arrange
        const string json = "{\"rel_before\":3600}";

        // Act
        var predicate = JsonSerializer.Deserialize<Predicate>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(predicate);
        Assert.IsInstanceOfType(predicate, typeof(PredicateBeforeRelativeTime));

        var relPredicate = (PredicateBeforeRelativeTime)predicate;
        Assert.AreEqual(3600, relPredicate.RelBefore);
    }

    /// <summary>
    ///     Verifies that Predicate can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithPredicate_RoundTripsCorrectly()
    {
        // Arrange
        var predicate = new PredicateAnd(
            new PredicateOr(
                new PredicateBeforeRelativeTime(12),
                new PredicateBeforeAbsoluteTime("2020-08-26T11:15:39Z")
            ),
            new PredicateNot(new PredicateUnconditional())
        );

        // Act
        var json = JsonSerializer.Serialize<Predicate>(predicate, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(json);

        // Verify the JSON structure is correct
        Assert.IsTrue(json.Contains("\"and\""), $"JSON should contain 'and' property. Actual: {json}");

        // Deserialize back and verify
        var deserialized = JsonSerializer.Deserialize<Predicate>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(deserialized);
        Assert.IsInstanceOfType(deserialized, typeof(PredicateAnd));
    }

    /// <summary>
    ///     Verifies that PredicateBeforeRelativeTime can be deserialized from JSON with string value.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithRelativeTimePredicateStringJson_ReturnsPredicateBeforeRelativeTime()
    {
        // Arrange
        const string json = "{\"rel_before\":\"3600\"}";

        // Act
        var predicate = JsonSerializer.Deserialize<Predicate>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(predicate);
        Assert.IsInstanceOfType(predicate, typeof(PredicateBeforeRelativeTime));

        var relPredicate = (PredicateBeforeRelativeTime)predicate;
        Assert.AreEqual(3600, relPredicate.RelBefore);
    }

    /// <summary>
    ///     Verifies that PredicateBeforeAbsoluteTime can be deserialized from JSON with string epoch.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAbsoluteTimePredicateStringEpochJson_ReturnsPredicateBeforeAbsoluteTime()
    {
        // Arrange
        const string json = "{\"abs_before\":\"2020-08-26T11:15:39Z\",\"abs_before_epoch\":\"1598440539\"}";

        // Act
        var predicate = JsonSerializer.Deserialize<Predicate>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(predicate);
        Assert.IsInstanceOfType(predicate, typeof(PredicateBeforeAbsoluteTime));

        var absPredicate = (PredicateBeforeAbsoluteTime)predicate;
        Assert.AreEqual(1598440539, absPredicate.AbsBeforeEpoch);
    }
}