using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Claimant;

public class ClaimPredicateOr : ClaimPredicate
{
    public ClaimPredicateOr(ClaimPredicate leftPredicate, ClaimPredicate rightPredicate)
    {
        LeftPredicate = leftPredicate;
        RightPredicate = rightPredicate;
    }

    public ClaimPredicate LeftPredicate { get; }
    public ClaimPredicate RightPredicate { get; }

    public override Xdr.ClaimPredicate ToXdr()
    {
        return new Xdr.ClaimPredicate
        {
            Discriminant = new ClaimPredicateType
            {
                InnerValue = ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_OR
            },
            OrPredicates = new[] { LeftPredicate.ToXdr(), RightPredicate.ToXdr() }
        };
    }
}