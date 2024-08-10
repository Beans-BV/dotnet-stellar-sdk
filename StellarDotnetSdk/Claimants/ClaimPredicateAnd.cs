using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Claimants;

public class ClaimPredicateAnd : ClaimPredicate
{
    public ClaimPredicateAnd(ClaimPredicate leftPredicate, ClaimPredicate rightPredicate)
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
                InnerValue = ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_AND,
            },
            AndPredicates = new[] { LeftPredicate.ToXdr(), RightPredicate.ToXdr() },
        };
    }
}