using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Claimants;

/// <summary>
///     Represents an unconditional claim predicate that is always satisfied.
///     A claimable balance with this predicate can be claimed at any time without restrictions.
/// </summary>
public class ClaimPredicateUnconditional : ClaimPredicate
{
    /// <inheritdoc />
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