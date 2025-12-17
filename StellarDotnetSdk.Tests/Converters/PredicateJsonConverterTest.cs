using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public void TestRead_Unconditional()
    {
        var result = JsonSerializer.Deserialize<Predicate>(@"{""unconditional"":true}", _options);

        Assert.IsInstanceOfType(result, typeof(PredicateUnconditional));
    }

    /// <summary>
    ///     Tests deserialization of before absolute time predicate.
    ///     Verifies that JSON with abs_before property deserializes to PredicateBeforeAbsoluteTime.
    /// </summary>
    [TestMethod]
    public void TestRead_BeforeAbsoluteTime()
    {
        var result = JsonSerializer.Deserialize<Predicate>(@"{""abs_before"":""2025-12-31T23:59:59Z""}", _options);

        Assert.IsInstanceOfType(result, typeof(PredicateBeforeAbsoluteTime));
        Assert.AreEqual("2025-12-31T23:59:59Z", ((PredicateBeforeAbsoluteTime)result!).AbsBefore);
    }

    /// <summary>
    ///     Tests deserialization of before absolute time predicate with epoch value.
    ///     Verifies that abs_before_epoch numeric value is correctly parsed.
    /// </summary>
    [TestMethod]
    public void TestRead_BeforeAbsoluteTime_WithEpoch()
    {
        var result = JsonSerializer.Deserialize<Predicate>(@"{""abs_before"":""2025-12-31T23:59:59Z"",""abs_before_epoch"":1767225599}", _options);

        var abs = (PredicateBeforeAbsoluteTime)result!;
        Assert.AreEqual(1767225599L, abs.AbsBeforeEpoch);
    }

    /// <summary>
    ///     Tests deserialization of before absolute time predicate with string epoch value.
    ///     Verifies that abs_before_epoch string value is correctly parsed to long.
    /// </summary>
    [TestMethod]
    public void TestRead_BeforeAbsoluteTime_WithStringEpoch()
    {
        // Tests the string parsing branch for abs_before_epoch
        var result = JsonSerializer.Deserialize<Predicate>(@"{""abs_before"":""2025-12-31T23:59:59Z"",""abs_before_epoch"":""1767225599""}", _options);

        Assert.AreEqual(1767225599L, ((PredicateBeforeAbsoluteTime)result!).AbsBeforeEpoch);
    }

    /// <summary>
    ///     Tests deserialization of before relative time predicate.
    ///     Verifies that JSON with rel_before property deserializes to PredicateBeforeRelativeTime.
    /// </summary>
    [TestMethod]
    public void TestRead_BeforeRelativeTime()
    {
        var result = JsonSerializer.Deserialize<Predicate>(@"{""rel_before"":3600}", _options);

        Assert.IsInstanceOfType(result, typeof(PredicateBeforeRelativeTime));
        Assert.AreEqual(3600L, ((PredicateBeforeRelativeTime)result!).RelBefore);
    }

    /// <summary>
    ///     Tests deserialization of before relative time predicate with string value.
    ///     Verifies that rel_before string value is correctly parsed to long.
    /// </summary>
    [TestMethod]
    public void TestRead_BeforeRelativeTime_StringValue()
    {
        // Tests the string parsing branch for rel_before
        var result = JsonSerializer.Deserialize<Predicate>(@"{""rel_before"":""3600""}", _options);

        Assert.AreEqual(3600L, ((PredicateBeforeRelativeTime)result!).RelBefore);
    }

    /// <summary>
    ///     Tests deserialization of AND predicate.
    ///     Verifies that JSON with and array property deserializes to PredicateAnd with correct left and right predicates.
    /// </summary>
    [TestMethod]
    public void TestRead_And()
    {
        var result = JsonSerializer.Deserialize<Predicate>(@"{""and"":[{""unconditional"":true},{""rel_before"":3600}]}", _options);

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
    public void TestRead_Or()
    {
        var result = JsonSerializer.Deserialize<Predicate>(@"{""or"":[{""unconditional"":true},{""rel_before"":3600}]}", _options);

        Assert.IsInstanceOfType(result, typeof(PredicateOr));
    }

    /// <summary>
    ///     Tests deserialization of NOT predicate.
    ///     Verifies that JSON with not property deserializes to PredicateNot with correct inner predicate.
    /// </summary>
    [TestMethod]
    public void TestRead_Not()
    {
        var result = JsonSerializer.Deserialize<Predicate>(@"{""not"":{""unconditional"":true}}", _options);

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
    public void TestWrite_Unconditional()
    {
        var json = JsonSerializer.Serialize<Predicate>(new PredicateUnconditional(), _options);

        Assert.IsTrue(json.Contains("\"unconditional\":true"));
    }

    /// <summary>
    ///     Tests serialization of before absolute time predicate with epoch value.
    ///     Verifies that PredicateBeforeAbsoluteTime with epoch serializes with both abs_before and abs_before_epoch properties.
    /// </summary>
    [TestMethod]
    public void TestWrite_BeforeAbsoluteTime_WithEpoch()
    {
        var json = JsonSerializer.Serialize<Predicate>(new PredicateBeforeAbsoluteTime("2025-12-31T23:59:59Z", 1767225599L), _options);

        Assert.IsTrue(json.Contains("\"abs_before\":\"2025-12-31T23:59:59Z\""));
        Assert.IsTrue(json.Contains("\"abs_before_epoch\":1767225599"));
    }

    /// <summary>
    ///     Tests serialization of before absolute time predicate without epoch value.
    ///     Verifies that PredicateBeforeAbsoluteTime without epoch omits abs_before_epoch property.
    /// </summary>
    [TestMethod]
    public void TestWrite_BeforeAbsoluteTime_WithoutEpoch()
    {
        var json = JsonSerializer.Serialize<Predicate>(new PredicateBeforeAbsoluteTime("2025-12-31T23:59:59Z"), _options);

        Assert.IsTrue(json.Contains("\"abs_before\":"));
        Assert.IsFalse(json.Contains("abs_before_epoch"));
    }

    /// <summary>
    ///     Tests serialization of before relative time predicate.
    ///     Verifies that PredicateBeforeRelativeTime serializes to JSON with rel_before property.
    /// </summary>
    [TestMethod]
    public void TestWrite_BeforeRelativeTime()
    {
        var json = JsonSerializer.Serialize<Predicate>(new PredicateBeforeRelativeTime(3600), _options);

        Assert.IsTrue(json.Contains("\"rel_before\":3600"));
    }

    /// <summary>
    ///     Tests serialization of AND predicate.
    ///     Verifies that PredicateAnd serializes to JSON with and array property.
    /// </summary>
    [TestMethod]
    public void TestWrite_And()
    {
        var predicate = new PredicateAnd(new PredicateUnconditional(), new PredicateBeforeRelativeTime(3600));

        var json = JsonSerializer.Serialize<Predicate>(predicate, _options);

        Assert.IsTrue(json.Contains("\"and\":"));
    }

    /// <summary>
    ///     Tests serialization of OR predicate.
    ///     Verifies that PredicateOr serializes to JSON with or array property.
    /// </summary>
    [TestMethod]
    public void TestWrite_Or()
    {
        var predicate = new PredicateOr(new PredicateUnconditional(), new PredicateBeforeRelativeTime(3600));

        var json = JsonSerializer.Serialize<Predicate>(predicate, _options);

        Assert.IsTrue(json.Contains("\"or\":"));
    }

    /// <summary>
    ///     Tests serialization of NOT predicate.
    ///     Verifies that PredicateNot serializes to JSON with not property.
    /// </summary>
    [TestMethod]
    public void TestWrite_Not()
    {
        var predicate = new PredicateNot(new PredicateUnconditional());

        var json = JsonSerializer.Serialize<Predicate>(predicate, _options);

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
    public void TestRead_ThrowsForUnknownPredicateType()
    {
        JsonSerializer.Deserialize<Predicate>(@"{""unknown"":true}", _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for empty object.
    ///     Verifies proper error handling when JSON object has no predicate properties.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestRead_ThrowsForEmptyObject()
    {
        JsonSerializer.Deserialize<Predicate>(@"{}", _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for empty abs_before value.
    ///     Verifies validation for non-empty abs_before string.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestRead_ThrowsForEmptyAbsBefore()
    {
        JsonSerializer.Deserialize<Predicate>(@"{""abs_before"":""""}", _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when and array has less than two predicates.
    ///     Verifies validation for minimum array length requirement.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestRead_ThrowsForAndWithLessThanTwoPredicates()
    {
        JsonSerializer.Deserialize<Predicate>(@"{""and"":[{""unconditional"":true}]}", _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when or array has less than two predicates.
    ///     Verifies validation for minimum array length requirement.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestRead_ThrowsForOrWithLessThanTwoPredicates()
    {
        JsonSerializer.Deserialize<Predicate>(@"{""or"":[{""unconditional"":true}]}", _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when not property is null.
    ///     Verifies validation for non-null inner predicate.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestRead_ThrowsForNotWithNullInner()
    {
        JsonSerializer.Deserialize<Predicate>(@"{""not"":null}", _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when and array contains null predicate.
    ///     Verifies validation for non-null predicates in array.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestRead_ThrowsForAndWithNullInArray()
    {
        JsonSerializer.Deserialize<Predicate>(@"{""and"":[{""unconditional"":true},null]}", _options);
    }

    /// <summary>
    ///     Tests that serialization throws JsonException for unknown predicate type.
    ///     Verifies proper error handling when predicate type is not recognized.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestWrite_ThrowsForUnknownPredicateType()
    {
        var options = new JsonSerializerOptions { Converters = { new PredicateJsonConverter() } };
        JsonSerializer.Serialize<Predicate>(new UnknownPredicate(), options);
    }

    private sealed class UnknownPredicate : Predicate
    {
        public override Claimants.ClaimPredicate ToClaimPredicate() => throw new NotImplementedException();
    }

    #endregion
}
