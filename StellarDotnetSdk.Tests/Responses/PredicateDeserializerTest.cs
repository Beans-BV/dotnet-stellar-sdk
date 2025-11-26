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
            "{\"and\":[{\"or\":[{\"rel_before\":12},{\"abs_before\":\"2020-08-26T11:15:39Z\"}]},{\"not\":{\"unconditional\":true}}]}";
        var predicate = JsonSerializer.Deserialize<Predicate>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(predicate);
        var claimPredicate = predicate.ToClaimPredicate();

        var andPredicate = (ClaimPredicateAnd)claimPredicate;
        Assert.IsNotNull(andPredicate);

        var orPredicate = (ClaimPredicateOr)andPredicate.LeftPredicate;
        Assert.IsNotNull(orPredicate);
        var notPredicate = (ClaimPredicateNot)andPredicate.RightPredicate;
        Assert.IsNotNull(notPredicate);

        var relBefore = (ClaimPredicateBeforeRelativeTime)orPredicate.LeftPredicate;
        Assert.IsNotNull(relBefore);
        var absBefore = (ClaimPredicateBeforeAbsoluteTime)orPredicate.RightPredicate;
        Assert.IsNotNull(absBefore);

        var unconditional = (ClaimPredicateUnconditional)notPredicate.Predicate;
        Assert.IsNotNull(unconditional);
    }
}