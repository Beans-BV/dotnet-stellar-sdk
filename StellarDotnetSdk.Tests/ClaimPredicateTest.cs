using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Claimant;
using xdrSDK = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class ClaimPredicateTest
{
    [TestMethod]
    public void TestClaimPredicateBeforeAbsoluteTime()
    {
        var predicate = ClaimPredicate.BeforeAbsoluteTime(new xdrSDK.TimePoint(new xdrSDK.Uint64(1600720493)));
        var xdr = predicate.ToXdr();

        var parsed = (ClaimPredicateBeforeAbsoluteTime)ClaimPredicate.FromXdr(xdr);

        Assert.AreEqual(1600720493, parsed.DateTime.ToUnixTimeSeconds());
    }

    [TestMethod]
    public void TestClaimPredicateBeforeAbsoluteTimeMaxInt()
    {
        var predicate = ClaimPredicate.BeforeAbsoluteTime(new xdrSDK.TimePoint(new xdrSDK.Uint64(ulong.MaxValue)));
        var xdr = predicate.ToXdr();

        var parsed = (ClaimPredicateBeforeAbsoluteTime)ClaimPredicate.FromXdr(xdr);

        Assert.AreEqual(ulong.MaxValue, parsed.TimePoint.InnerValue.InnerValue);
    }

    [TestMethod]
    public void TestClaimPredicateBeforeRelativeTime()
    {
        var predicate = ClaimPredicate.BeforeRelativeTime(new xdrSDK.Duration(new xdrSDK.Uint64(120)));
        var xdr = predicate.ToXdr();

        var parsed = (ClaimPredicateBeforeRelativeTime)ClaimPredicate.FromXdr(xdr);

        Assert.AreEqual(120.0, parsed.Duration);
    }

    [TestMethod]
    public void TestClaimPredicateNot()
    {
        var predicate =
            ClaimPredicate.Not(ClaimPredicate.BeforeRelativeTime(new xdrSDK.Duration(new xdrSDK.Uint64(120))));
        var xdr = predicate.ToXdr();

        var parsed = (ClaimPredicateNot)ClaimPredicate.FromXdr(xdr);

        Assert.IsNotNull(parsed.Predicate);
    }

    [TestMethod]
    public void TestClaimPredicateAnd()
    {
        var predicate = ClaimPredicate.And(
            ClaimPredicate.BeforeRelativeTime(new xdrSDK.Duration(new xdrSDK.Uint64(120))),
            ClaimPredicate.BeforeRelativeTime(new xdrSDK.Duration(new xdrSDK.Uint64(240))));
        var xdr = predicate.ToXdr();

        var parsed = (ClaimPredicateAnd)ClaimPredicate.FromXdr(xdr);

        Assert.IsNotNull(parsed.LeftPredicate);
        Assert.IsNotNull(parsed.RightPredicate);
    }

    [TestMethod]
    public void TestClaimPredicateOr()
    {
        var predicate = ClaimPredicate.Or(
            ClaimPredicate.BeforeRelativeTime(new xdrSDK.Duration(new xdrSDK.Uint64(120))),
            ClaimPredicate.BeforeRelativeTime(new xdrSDK.Duration(new xdrSDK.Uint64(240))));
        var xdr = predicate.ToXdr();

        var parsed = (ClaimPredicateOr)ClaimPredicate.FromXdr(xdr);

        Assert.IsNotNull(parsed.LeftPredicate);
        Assert.IsNotNull(parsed.RightPredicate);
    }

    [TestMethod]
    public void TestClaimPredicateUnconditional()
    {
        var predicate = ClaimPredicate.Unconditional();
        var xdr = predicate.ToXdr();

        var parsed = (ClaimPredicateUnconditional)ClaimPredicate.FromXdr(xdr);

        Assert.IsNotNull(parsed);
    }
}