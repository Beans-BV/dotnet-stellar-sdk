using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Claimants;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Predicates;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for PredicateJsonConverter.
///     Focus: polymorphic type selection based on JSON property presence.
/// </summary>
[TestClass]
public class PredicateJsonConverterTest
{
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    #region Read Tests - One per predicate type

    /// <summary>
    ///     Tests deserialization of unconditional predicate.
    ///     Verifies that JSON with unconditional property deserializes to PredicateUnconditional.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithUnconditionalJson_ReturnsPredicateUnconditional()
    {
        // Arrange
        var json = @"{""unconditional"":true}";

        // Act
        var result = JsonSerializer.Deserialize<Predicate>(json, _options);

        // Assert
        Assert.IsInstanceOfType(result, typeof(PredicateUnconditional));
    }

    /// <summary>
    ///     Tests deserialization of before absolute time predicate.
    ///     Verifies that JSON with abs_before property deserializes to PredicateBeforeAbsoluteTime.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithBeforeAbsoluteTimeJson_ReturnsPredicateBeforeAbsoluteTime()
    {
        // Arrange
        var json = @"{""abs_before"":""2025-12-31T23:59:59Z""}";

        // Act
        var result = JsonSerializer.Deserialize<Predicate>(json, _options);

        // Assert
        Assert.IsInstanceOfType(result, typeof(PredicateBeforeAbsoluteTime));
        Assert.AreEqual("2025-12-31T23:59:59Z", ((PredicateBeforeAbsoluteTime)result!).AbsBefore);
    }

    /// <summary>
    ///     Tests deserialization of before absolute time predicate with epoch value.
    ///     Verifies that abs_before_epoch numeric value is correctly parsed.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithBeforeAbsoluteTimeAndEpoch_ReturnsPredicateWithEpoch()
    {
        // Arrange
        var json = @"{""abs_before"":""2025-12-31T23:59:59Z"",""abs_before_epoch"":1767225599}";

        // Act
        var result = JsonSerializer.Deserialize<Predicate>(json, _options);

        // Assert
        var abs = (PredicateBeforeAbsoluteTime)result!;
        Assert.AreEqual(1767225599L, abs.AbsBeforeEpoch);
    }

    /// <summary>
    ///     Tests deserialization of before absolute time predicate with string epoch value.
    ///     Verifies that abs_before_epoch string value is correctly parsed to long.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithBeforeAbsoluteTimeAndStringEpoch_ReturnsPredicateWithEpoch()
    {
        // Arrange - Tests the string parsing branch for abs_before_epoch
        var json = @"{""abs_before"":""2025-12-31T23:59:59Z"",""abs_before_epoch"":""1767225599""}";

        // Act
        var result = JsonSerializer.Deserialize<Predicate>(json, _options);

        // Assert
        Assert.AreEqual(1767225599L, ((PredicateBeforeAbsoluteTime)result!).AbsBeforeEpoch);
    }

    /// <summary>
    ///     Tests deserialization of before relative time predicate.
    ///     Verifies that JSON with rel_before property deserializes to PredicateBeforeRelativeTime.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithBeforeRelativeTimeJson_ReturnsPredicateBeforeRelativeTime()
    {
        // Arrange
        var json = @"{""rel_before"":3600}";

        // Act
        var result = JsonSerializer.Deserialize<Predicate>(json, _options);

        // Assert
        Assert.IsInstanceOfType(result, typeof(PredicateBeforeRelativeTime));
        Assert.AreEqual(3600L, ((PredicateBeforeRelativeTime)result!).RelBefore);
    }

    /// <summary>
    ///     Tests deserialization of before relative time predicate with string value.
    ///     Verifies that rel_before string value is correctly parsed to long.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithBeforeRelativeTimeStringValue_ReturnsPredicateWithRelBefore()
    {
        // Arrange - Tests the string parsing branch for rel_before
        var json = @"{""rel_before"":""3600""}";

        // Act
        var result = JsonSerializer.Deserialize<Predicate>(json, _options);

        // Assert
        Assert.AreEqual(3600L, ((PredicateBeforeRelativeTime)result!).RelBefore);
    }

    /// <summary>
    ///     Tests deserialization of AND predicate.
    ///     Verifies that JSON with and array property deserializes to PredicateAnd with correct left and right predicates.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAndJson_ReturnsPredicateAnd()
    {
        // Arrange
        var json = @"{""and"":[{""unconditional"":true},{""rel_before"":3600}]}";

        // Act
        var result = JsonSerializer.Deserialize<Predicate>(json, _options);

        // Assert
        Assert.IsInstanceOfType(result, typeof(PredicateAnd));
        var and = (PredicateAnd)result!;
        Assert.IsInstanceOfType(and.Left, typeof(PredicateUnconditional));
        Assert.IsInstanceOfType(and.Right, typeof(PredicateBeforeRelativeTime));
    }

    /// <summary>
    ///     Tests deserialization of OR predicate.
    ///     Verifies that JSON with or array property deserializes to PredicateOr.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithOrJson_ReturnsPredicateOr()
    {
        // Arrange
        var json = @"{""or"":[{""unconditional"":true},{""rel_before"":3600}]}";

        // Act
        var result = JsonSerializer.Deserialize<Predicate>(json, _options);

        // Assert
        Assert.IsInstanceOfType(result, typeof(PredicateOr));
    }

    /// <summary>
    ///     Tests deserialization of NOT predicate.
    ///     Verifies that JSON with not property deserializes to PredicateNot with correct inner predicate.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithNotJson_ReturnsPredicateNot()
    {
        // Arrange
        var json = @"{""not"":{""unconditional"":true}}";

        // Act
        var result = JsonSerializer.Deserialize<Predicate>(json, _options);

        // Assert
        Assert.IsInstanceOfType(result, typeof(PredicateNot));
        Assert.IsInstanceOfType(((PredicateNot)result!).Inner, typeof(PredicateUnconditional));
    }

    #endregion

    #region Write Tests - One per predicate type

    /// <summary>
    ///     Tests serialization of unconditional predicate.
    ///     Verifies that PredicateUnconditional serializes to JSON with unconditional property set to true.
    /// </summary>
    [TestMethod]
    public void Serialize_WithPredicateUnconditional_ProducesCorrectJson()
    {
        // Arrange
        var predicate = new PredicateUnconditional();

        // Act
        var json = JsonSerializer.Serialize<Predicate>(predicate, _options);

        // Assert
        Assert.IsTrue(json.Contains("\"unconditional\":true"));
    }

    /// <summary>
    ///     Tests serialization of before absolute time predicate with epoch value.
    ///     Verifies that PredicateBeforeAbsoluteTime with epoch serializes with both abs_before and abs_before_epoch
    ///     properties.
    /// </summary>
    [TestMethod]
    public void Serialize_WithBeforeAbsoluteTimeAndEpoch_ProducesCorrectJson()
    {
        // Arrange
        var predicate = new PredicateBeforeAbsoluteTime("2025-12-31T23:59:59Z", 1767225599L);

        // Act
        var json = JsonSerializer.Serialize<Predicate>(predicate, _options);

        // Assert
        Assert.IsTrue(json.Contains("\"abs_before\":\"2025-12-31T23:59:59Z\""));
        Assert.IsTrue(json.Contains("\"abs_before_epoch\":1767225599"));
    }

    /// <summary>
    ///     Tests serialization of before absolute time predicate without epoch value.
    ///     Verifies that PredicateBeforeAbsoluteTime without epoch omits abs_before_epoch property.
    /// </summary>
    [TestMethod]
    public void Serialize_WithBeforeAbsoluteTimeWithoutEpoch_OmitsEpochProperty()
    {
        // Arrange
        var predicate = new PredicateBeforeAbsoluteTime("2025-12-31T23:59:59Z");

        // Act
        var json = JsonSerializer.Serialize<Predicate>(predicate, _options);

        // Assert
        Assert.IsTrue(json.Contains("\"abs_before\":"));
        Assert.IsFalse(json.Contains("abs_before_epoch"));
    }

    /// <summary>
    ///     Tests serialization of before relative time predicate.
    ///     Verifies that PredicateBeforeRelativeTime serializes to JSON with rel_before property.
    /// </summary>
    [TestMethod]
    public void Serialize_WithBeforeRelativeTime_ProducesCorrectJson()
    {
        // Arrange
        var predicate = new PredicateBeforeRelativeTime(3600);

        // Act
        var json = JsonSerializer.Serialize<Predicate>(predicate, _options);

        // Assert
        Assert.IsTrue(json.Contains("\"rel_before\":3600"));
    }

    /// <summary>
    ///     Tests serialization of AND predicate.
    ///     Verifies that PredicateAnd serializes to JSON with and array property.
    /// </summary>
    [TestMethod]
    public void Serialize_WithPredicateAnd_ProducesCorrectJson()
    {
        // Arrange
        var predicate = new PredicateAnd(new PredicateUnconditional(), new PredicateBeforeRelativeTime(3600));

        // Act
        var json = JsonSerializer.Serialize<Predicate>(predicate, _options);

        // Assert
        Assert.IsTrue(json.Contains("\"and\":"));
    }

    /// <summary>
    ///     Tests serialization of OR predicate.
    ///     Verifies that PredicateOr serializes to JSON with or array property.
    /// </summary>
    [TestMethod]
    public void Serialize_WithPredicateOr_ProducesCorrectJson()
    {
        // Arrange
        var predicate = new PredicateOr(new PredicateUnconditional(), new PredicateBeforeRelativeTime(3600));

        // Act
        var json = JsonSerializer.Serialize<Predicate>(predicate, _options);

        // Assert
        Assert.IsTrue(json.Contains("\"or\":"));
    }

    /// <summary>
    ///     Tests serialization of NOT predicate.
    ///     Verifies that PredicateNot serializes to JSON with not property.
    /// </summary>
    [TestMethod]
    public void Serialize_WithPredicateNot_ProducesCorrectJson()
    {
        // Arrange
        var predicate = new PredicateNot(new PredicateUnconditional());

        // Act
        var json = JsonSerializer.Serialize<Predicate>(predicate, _options);

        // Assert
        Assert.IsTrue(json.Contains("\"not\":"));
    }

    #endregion

    #region Error Cases

    /// <summary>
    ///     Tests that deserialization throws JsonException for unknown predicate type.
    ///     Verifies proper error handling when JSON contains unrecognized predicate property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithUnknownPredicateType_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""unknown"":true}";

        // Act & Assert
        JsonSerializer.Deserialize<Predicate>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for empty object.
    ///     Verifies proper error handling when JSON object has no predicate properties.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithEmptyObject_ThrowsJsonException()
    {
        // Arrange
        var json = @"{}";

        // Act & Assert
        JsonSerializer.Deserialize<Predicate>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for empty abs_before value.
    ///     Verifies validation for non-empty abs_before string.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithEmptyAbsBefore_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""abs_before"":""""}";

        // Act & Assert
        JsonSerializer.Deserialize<Predicate>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when and array has less than two predicates.
    ///     Verifies validation for minimum array length requirement.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithAndArrayLessThanTwoPredicates_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""and"":[{""unconditional"":true}]}";

        // Act & Assert
        JsonSerializer.Deserialize<Predicate>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when or array has less than two predicates.
    ///     Verifies validation for minimum array length requirement.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithOrArrayLessThanTwoPredicates_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""or"":[{""unconditional"":true}]}";

        // Act & Assert
        JsonSerializer.Deserialize<Predicate>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when not property is null.
    ///     Verifies validation for non-null inner predicate.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithNotNullInner_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""not"":null}";

        // Act & Assert
        JsonSerializer.Deserialize<Predicate>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when and array contains null predicate.
    ///     Verifies validation for non-null predicates in array.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithAndArrayContainingNull_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""and"":[{""unconditional"":true},null]}";

        // Act & Assert
        JsonSerializer.Deserialize<Predicate>(json, _options);
    }

    /// <summary>
    ///     Tests that serialization throws JsonException for unknown predicate type.
    ///     Verifies proper error handling when predicate type is not recognized.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Serialize_WithUnknownPredicateType_ThrowsJsonException()
    {
        // Arrange
        var options = new JsonSerializerOptions { Converters = { new PredicateJsonConverter() } };
        var predicate = new UnknownPredicate();

        // Act & Assert
        JsonSerializer.Serialize<Predicate>(predicate, options);
    }

    private sealed class UnknownPredicate : Predicate
    {
        public override ClaimPredicate ToClaimPredicate()
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}