using System;
using StellarDotnetSdk.Xdr;
using xdr_Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Claimants;

public class ClaimPredicateBeforeRelativeTime : ClaimPredicate
{
    [Obsolete("Use the ClaimPredicateBeforeRelativeTime(long duration) constructor instead.")]
    public ClaimPredicateBeforeRelativeTime(Duration duration)
    {
        Duration = (long)duration.InnerValue.InnerValue;
    }

    [Obsolete("Use the ClaimPredicateBeforeRelativeTime(long duration) constructor instead.")]
    public ClaimPredicateBeforeRelativeTime(ulong duration)
    {
        Duration = (long)duration;
    }

    public ClaimPredicateBeforeRelativeTime(long duration)
    {
        Duration = duration;
    }

    public long Duration { get; }

    public override Xdr.ClaimPredicate ToXdr()
    {
        return new Xdr.ClaimPredicate
        {
            Discriminant = new ClaimPredicateType
            {
                InnerValue = ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_BEFORE_RELATIVE_TIME
            },
            RelBefore = new xdr_Int64(Duration)
        };
    }
}