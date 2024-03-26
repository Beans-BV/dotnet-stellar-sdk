using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ClaimPredicateAnd : ClaimPredicate
{
    public ClaimPredicateAnd(ClaimPredicate leftPredicate, ClaimPredicate rightPredicate)
    {
        LeftPredicate = leftPredicate;
        RightPredicate = rightPredicate;
    }

    public ClaimPredicate LeftPredicate { get; }
    public ClaimPredicate RightPredicate { get; }

    public override xdr.ClaimPredicate ToXdr()
    {
        return new xdr.ClaimPredicate
        {
            Discriminant = new ClaimPredicateType
            {
                InnerValue = ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_AND
            },
            AndPredicates = new[] { LeftPredicate.ToXdr(), RightPredicate.ToXdr() }
        };
    }
}