using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Claimant;

public class ClaimPredicateNot : ClaimPredicate
{
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
                InnerValue = ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_NOT
            },
            NotPredicate = Predicate.ToXdr()
        };
    }
}