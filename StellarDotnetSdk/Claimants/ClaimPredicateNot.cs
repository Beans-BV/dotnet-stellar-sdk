using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Claimants;

/// <summary>
///     Represents a logical NOT (negation) of a claim predicate.
///     A claimable balance can be claimed only when the inner predicate is <em>not</em> satisfied.
/// </summary>
public class ClaimPredicateNot : ClaimPredicate
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClaimPredicateNot" /> class that negates the given predicate.
    /// </summary>
    /// <param name="predicate">The predicate to negate.</param>
    public ClaimPredicateNot(ClaimPredicate predicate)
    {
        Predicate = predicate;
    }

    public ClaimPredicate Predicate { get; }

    public override Xdr.ClaimPredicate ToXdr()
    {
        return new Xdr.ClaimPredicate
        {
            Discriminant = new ClaimPredicateType
            {
                InnerValue = ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_NOT,
            },
            NotPredicate = Predicate.ToXdr(),
        };
    }
}