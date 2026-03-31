using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Claimants;

/// <summary>
///     Represents a logical AND combination of two claim predicates.
///     A claimable balance can be claimed only when both the left and right predicates are satisfied.
/// </summary>
public class ClaimPredicateAnd : ClaimPredicate
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClaimPredicateAnd" /> class with two predicates
    ///     that must both be satisfied for the claim to succeed.
    /// </summary>
    /// <param name="leftPredicate">The first predicate that must be satisfied.</param>
    /// <param name="rightPredicate">The second predicate that must be satisfied.</param>
    public ClaimPredicateAnd(ClaimPredicate leftPredicate, ClaimPredicate rightPredicate)
    {
        LeftPredicate = leftPredicate;
        RightPredicate = rightPredicate;
    }

    /// <summary>Gets the first predicate that must be satisfied.</summary>
    public ClaimPredicate LeftPredicate { get; }

    /// <summary>Gets the second predicate that must be satisfied.</summary>
    public ClaimPredicate RightPredicate { get; }

    /// <inheritdoc />
    public override Xdr.ClaimPredicate ToXdr()
    {
        return new Xdr.ClaimPredicate
        {
            Discriminant = new ClaimPredicateType
            {
                InnerValue = ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_AND,
            },
            AndPredicates = new[] { LeftPredicate.ToXdr(), RightPredicate.ToXdr() },
        };
    }
}