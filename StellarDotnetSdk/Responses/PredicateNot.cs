using ClaimPredicate = StellarDotnetSdk.Claimants.ClaimPredicate;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a logical NOT predicate that negates its inner predicate.
/// </summary>
public sealed class PredicateNot : Predicate
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PredicateNot" /> class.
    /// </summary>
    /// <param name="inner">The predicate to negate.</param>
    public PredicateNot(Predicate inner)
    {
        Inner = inner;
    }

    /// <summary>
    ///     The inner predicate that is negated.
    /// </summary>
    public Predicate Inner { get; }

    /// <inheritdoc />
    public override ClaimPredicate ToClaimPredicate()
    {
        return ClaimPredicate.Not(Inner.ToClaimPredicate());
    }
}