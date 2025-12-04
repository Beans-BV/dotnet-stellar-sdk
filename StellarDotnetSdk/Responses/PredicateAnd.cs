using ClaimPredicate = StellarDotnetSdk.Claimants.ClaimPredicate;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a logical AND predicate that requires both child predicates to be true.
/// </summary>
public sealed class PredicateAnd : Predicate
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PredicateAnd" /> class.
    /// </summary>
    /// <param name="left">The left predicate.</param>
    /// <param name="right">The right predicate.</param>
    public PredicateAnd(Predicate left, Predicate right)
    {
        Left = left;
        Right = right;
    }

    /// <summary>
    ///     The left predicate of the AND condition.
    /// </summary>
    public Predicate Left { get; }

    /// <summary>
    ///     The right predicate of the AND condition.
    /// </summary>
    public Predicate Right { get; }

    /// <inheritdoc />
    public override ClaimPredicate ToClaimPredicate()
    {
        return ClaimPredicate.And(Left.ToClaimPredicate(), Right.ToClaimPredicate());
    }
}