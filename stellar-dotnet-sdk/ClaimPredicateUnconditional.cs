using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ClaimPredicateUnconditional : ClaimPredicate
{
    public override xdr.ClaimPredicate ToXdr()
    {
        return new xdr.ClaimPredicate
        {
            Discriminant = new ClaimPredicateType
            {
                InnerValue = ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_UNCONDITIONAL
            }
        };
    }
}