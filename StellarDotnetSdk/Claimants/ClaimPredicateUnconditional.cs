using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Claimants;

public class ClaimPredicateUnconditional : ClaimPredicate
{
    public override Xdr.ClaimPredicate ToXdr()
    {
        return new Xdr.ClaimPredicate
        {
            Discriminant = new ClaimPredicateType
            {
                InnerValue = ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_UNCONDITIONAL,
            },
        };
    }
}