using System;
using Newtonsoft.Json;
using StellarDotnetSdk.Xdr;
using claimant_ClaimPredicate = StellarDotnetSdk.Claimants.ClaimPredicate;

namespace StellarDotnetSdk.Responses;

public class Predicate
{
    [JsonProperty(PropertyName = "and")] public Predicate[]? And { get; init; }

    [JsonProperty(PropertyName = "or")] public Predicate[]? Or { get; init; }

    [JsonProperty(PropertyName = "not")] public Predicate? Not { get; init; }

    [JsonProperty(PropertyName = "unconditional")]
    public bool Unconditional { get; init; }

    [JsonProperty(PropertyName = "abs_before")]
    public string? AbsBefore { get; init; }

    [JsonProperty(PropertyName = "rel_before")]
    public long? RelBefore { get; init; }

    public claimant_ClaimPredicate ToClaimPredicate()
    {
        if (And is { Length: > 0 })
        {
            var leftPredicate = And[0].ToClaimPredicate();
            var rightPredicate = And[1].ToClaimPredicate();
            return claimant_ClaimPredicate.And(leftPredicate, rightPredicate);
        }

        if (Or is { Length: > 0 })
        {
            var leftPredicate = Or[0].ToClaimPredicate();
            var rightPredicate = Or[1].ToClaimPredicate();
            return claimant_ClaimPredicate.Or(leftPredicate, rightPredicate);
        }

        if (Not != null)
        {
            var predicate = Not.ToClaimPredicate();
            return claimant_ClaimPredicate.Not(predicate);
        }

        if (Unconditional)
        {
            return claimant_ClaimPredicate.Unconditional();
        }

        if (AbsBefore != null)
        {
            var dateTime = DateTimeOffset.Parse(AbsBefore);
            return claimant_ClaimPredicate.BeforeAbsoluteTime(dateTime);
        }

        if (RelBefore != null)
        {
            return claimant_ClaimPredicate.BeforeRelativeTime(new Duration(new Uint64((ulong)RelBefore.Value)));
        }

        throw new Exception("Invalid Predicate");
    }
}