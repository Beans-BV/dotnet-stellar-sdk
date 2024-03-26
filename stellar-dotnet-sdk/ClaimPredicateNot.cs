using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ClaimPredicateNot : ClaimPredicate
{
    public ClaimPredicateNot(ClaimPredicate predicate)
    {
        Predicate = predicate;
    }

    public ClaimPredicate Predicate { get; }

    public override xdr.ClaimPredicate ToXdr()
    {
        return new xdr.ClaimPredicate
        {
            Discriminant = new ClaimPredicateType
            {
                InnerValue = ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_NOT
            },
            NotPredicate = Predicate.ToXdr()
        };
    }
}