using System;
using Newtonsoft.Json;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk.responses;

public class Predicate
{
    public Predicate(Predicate[] and, Predicate[] or, Predicate not, bool unconditional, string absBefore,
        long? relBefore)
    {
        And = and;
        Or = or;
        Not = not;
        Unconditional = unconditional;
        AbsBefore = absBefore;
        RelBefore = relBefore;
    }

    [JsonProperty(PropertyName = "and")] public Predicate[]? And { get; set; }

    [JsonProperty(PropertyName = "or")] public Predicate[]? Or { get; set; }

    [JsonProperty(PropertyName = "not")] public Predicate? Not { get; set; }

    [JsonProperty(PropertyName = "unconditional")]
    public bool Unconditional { get; }

    [JsonProperty(PropertyName = "abs_before")]
    public string? AbsBefore { get; }

    [JsonProperty(PropertyName = "rel_before")]
    public long? RelBefore { get; }

    public ClaimPredicate ToClaimPredicate()
    {
        if (And != null && And.Length > 0)
        {
            var leftPredicate = And[0].ToClaimPredicate();
            var rightPredicate = And[1].ToClaimPredicate();
            return ClaimPredicate.And(leftPredicate, rightPredicate);
        }

        if (Or != null && Or.Length > 0)
        {
            var leftPredicate = Or[0].ToClaimPredicate();
            var rightPredicate = Or[1].ToClaimPredicate();
            return ClaimPredicate.Or(leftPredicate, rightPredicate);
        }

        if (Not != null)
        {
            var predicate = Not.ToClaimPredicate();
            return ClaimPredicate.Not(predicate);
        }

        if (Unconditional) return ClaimPredicate.Unconditional();

        if (AbsBefore != null)
        {
            var dateTime = DateTimeOffset.Parse(AbsBefore);
            return ClaimPredicate.BeforeAbsoluteTime(dateTime);
        }

        if (RelBefore != null)
            return ClaimPredicate.BeforeRelativeTime(new Duration(new Uint64((ulong)RelBefore.Value)));

        throw new Exception("Invalid Predicate");
    }
}