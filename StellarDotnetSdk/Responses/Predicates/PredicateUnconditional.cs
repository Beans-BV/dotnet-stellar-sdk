using ClaimPredicate = StellarDotnetSdk.Claimants.ClaimPredicate;

namespace StellarDotnetSdk.Responses.Predicates;

/// <summary>
///     Represents an unconditional predicate that is always satisfied.
/// </summary>
public sealed class PredicateUnconditional : Predicate
{
    /// <inheritdoc />
    public override ClaimPredicate ToClaimPredicate()
    {
        return ClaimPredicate.Unconditional();
    }
}