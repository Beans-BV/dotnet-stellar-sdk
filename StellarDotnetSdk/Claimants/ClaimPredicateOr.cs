using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Claimants;

/// <summary>
///     Represents a logical OR combination of two claim predicates.
///     A claimable balance can be claimed when either the left or right predicate (or both) is satisfied.
/// </summary>
public class ClaimPredicateOr : ClaimPredicate
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClaimPredicateOr" /> class with two predicates
    ///     where at least one must be satisfied for the claim to succeed.
    /// </summary>
    /// <param name="leftPredicate">The first predicate.</param>
    /// <param name="rightPredicate">The second predicate.</param>
    public ClaimPredicateOr(ClaimPredicate leftPredicate, ClaimPredicate rightPredicate)
    {
        LeftPredicate = leftPredicate;
        RightPredicate = rightPredicate;
    }

    /// <summary>Gets the first predicate.</summary>
    public ClaimPredicate LeftPredicate { get; }

    /// <summary>Gets the second predicate.</summary>
    public ClaimPredicate RightPredicate { get; }

    /// <inheritdoc />
    public override Xdr.ClaimPredicate ToXdr()
    {
        return new Xdr.ClaimPredicate
        {
            Discriminant = new ClaimPredicateType
            {
                InnerValue = ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_OR,
            },
            OrPredicates = new[] { LeftPredicate.ToXdr(), RightPredicate.ToXdr() },
        };
    }
}