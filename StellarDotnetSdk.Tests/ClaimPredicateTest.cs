using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Claimants;
using xdrSDK = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Unit tests for <see cref="ClaimPredicate" /> class.
/// </summary>
[TestClass]
public class ClaimPredicateTest
{
    /// <summary>
    ///     Verifies that ClaimPredicate.BeforeAbsoluteTime creates predicate that round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void BeforeAbsoluteTime_WithTimePoint_RoundTripsCorrectly()
    {
        // Arrange
        var predicate = ClaimPredicate.BeforeAbsoluteTime(new xdrSDK.TimePoint(new xdrSDK.Uint64(1600720493)));

        // Act
        var xdr = predicate.ToXdr();
        var parsed = (ClaimPredicateBeforeAbsoluteTime)ClaimPredicate.FromXdr(xdr);

        // Assert
        Assert.AreEqual(1600720493, parsed.DateTime.ToUnixTimeSeconds());
    }

    /// <summary>
    ///     Verifies that ClaimPredicate.BeforeAbsoluteTime with max ulong value round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void BeforeAbsoluteTime_WithMaxUlongValue_RoundTripsCorrectly()
    {
        // Arrange
        var predicate = ClaimPredicate.BeforeAbsoluteTime(new xdrSDK.TimePoint(new xdrSDK.Uint64(ulong.MaxValue)));

        // Act
        var xdr = predicate.ToXdr();
        var parsed = (ClaimPredicateBeforeAbsoluteTime)ClaimPredicate.FromXdr(xdr);

        // Assert
        Assert.AreEqual(ulong.MaxValue, parsed.TimePoint.InnerValue.InnerValue);
    }

    /// <summary>
    ///     Verifies that ClaimPredicate.BeforeRelativeTime creates predicate that round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void BeforeRelativeTime_WithDuration_RoundTripsCorrectly()
    {
        // Arrange
        var predicate = ClaimPredicate.BeforeRelativeTime(new xdrSDK.Duration(new xdrSDK.Uint64(120)));

        // Act
        var xdr = predicate.ToXdr();
        var parsed = (ClaimPredicateBeforeRelativeTime)ClaimPredicate.FromXdr(xdr);

        // Assert
        Assert.AreEqual(120.0, parsed.Duration);
    }

    /// <summary>
    ///     Verifies that ClaimPredicate.Not creates predicate that round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void Not_WithPredicate_RoundTripsCorrectly()
    {
        // Arrange
        var predicate =
            ClaimPredicate.Not(ClaimPredicate.BeforeRelativeTime(new xdrSDK.Duration(new xdrSDK.Uint64(120))));

        // Act
        var xdr = predicate.ToXdr();
        var parsed = (ClaimPredicateNot)ClaimPredicate.FromXdr(xdr);

        // Assert
        Assert.IsNotNull(parsed.Predicate);
    }

    /// <summary>
    ///     Verifies that ClaimPredicate.And creates predicate that round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void And_WithTwoPredicates_RoundTripsCorrectly()
    {
        // Arrange
        var predicate = ClaimPredicate.And(
            ClaimPredicate.BeforeRelativeTime(new xdrSDK.Duration(new xdrSDK.Uint64(120))),
            ClaimPredicate.BeforeRelativeTime(new xdrSDK.Duration(new xdrSDK.Uint64(240))));

        // Act
        var xdr = predicate.ToXdr();
        var parsed = (ClaimPredicateAnd)ClaimPredicate.FromXdr(xdr);

        // Assert
        Assert.IsNotNull(parsed.LeftPredicate);
        Assert.IsNotNull(parsed.RightPredicate);
    }

    /// <summary>
    ///     Verifies that ClaimPredicate.Or creates predicate that round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void Or_WithTwoPredicates_RoundTripsCorrectly()
    {
        // Arrange
        var predicate = ClaimPredicate.Or(
            ClaimPredicate.BeforeRelativeTime(new xdrSDK.Duration(new xdrSDK.Uint64(120))),
            ClaimPredicate.BeforeRelativeTime(new xdrSDK.Duration(new xdrSDK.Uint64(240))));

        // Act
        var xdr = predicate.ToXdr();
        var parsed = (ClaimPredicateOr)ClaimPredicate.FromXdr(xdr);

        // Assert
        Assert.IsNotNull(parsed.LeftPredicate);
        Assert.IsNotNull(parsed.RightPredicate);
    }

    /// <summary>
    ///     Verifies that ClaimPredicate.Unconditional creates predicate that round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void Unconditional_CreatesPredicate_RoundTripsCorrectly()
    {
        // Arrange
        var predicate = ClaimPredicate.Unconditional();

        // Act
        var xdr = predicate.ToXdr();
        var parsed = (ClaimPredicateUnconditional)ClaimPredicate.FromXdr(xdr);

        // Assert
        Assert.IsNotNull(parsed);
    }
}