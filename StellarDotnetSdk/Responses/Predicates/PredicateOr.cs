using ClaimPredicate = StellarDotnetSdk.Claimants.ClaimPredicate;

namespace StellarDotnetSdk.Responses.Predicates;

/// <summary>
///     Represents a logical OR predicate that requires at least one child predicate to be true.
/// </summary>
public sealed class PredicateOr : Predicate
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PredicateOr" /> class.
    /// </summary>
    /// <param name="left">The left predicate.</param>
    /// <param name="right">The right predicate.</param>
    public PredicateOr(Predicate left, Predicate right)
    {
        Left = left;
        Right = right;
    }

    /// <summary>
    ///     The left predicate of the OR condition.
    /// </summary>
    public Predicate Left { get; }

    /// <summary>
    ///     The right predicate of the OR condition.
    /// </summary>
    public Predicate Right { get; }

    /// <inheritdoc />
    public override ClaimPredicate ToClaimPredicate()
    {
        return ClaimPredicate.Or(Left.ToClaimPredicate(), Right.ToClaimPredicate());
    }
}