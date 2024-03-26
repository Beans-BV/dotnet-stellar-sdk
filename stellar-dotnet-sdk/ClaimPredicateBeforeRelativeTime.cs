using System;
using stellar_dotnet_sdk.xdr;
using Int64 = stellar_dotnet_sdk.xdr.Int64;

namespace stellar_dotnet_sdk;

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

    public override xdr.ClaimPredicate ToXdr()
    {
        return new xdr.ClaimPredicate
        {
            Discriminant = new ClaimPredicateType
            {
                InnerValue = ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_BEFORE_RELATIVE_TIME
            },
            RelBefore = new Int64(Duration)
        };
    }
}