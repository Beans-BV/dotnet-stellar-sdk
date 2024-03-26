using System;
using stellar_dotnet_sdk.xdr;
using Int64 = stellar_dotnet_sdk.xdr.Int64;

namespace stellar_dotnet_sdk;

public class ClaimPredicateBeforeAbsoluteTime : ClaimPredicate
{
    public ClaimPredicateBeforeAbsoluteTime(DateTimeOffset dateTime)
    {
        TimePoint.InnerValue = new Uint64((ulong)dateTime.ToUnixTimeSeconds());
    }

    public ClaimPredicateBeforeAbsoluteTime(TimePoint timePoint)
    {
        TimePoint = timePoint;
    }

    public ClaimPredicateBeforeAbsoluteTime(ulong timePoint)
    {
        TimePoint = new TimePoint(new Uint64(timePoint));
    }

    public DateTimeOffset DateTime => DateTimeOffset.FromUnixTimeSeconds((long)TimePoint.InnerValue.InnerValue);
    public TimePoint TimePoint { get; } = new();

    public override xdr.ClaimPredicate ToXdr()
    {
        return new xdr.ClaimPredicate
        {
            Discriminant = new ClaimPredicateType
            {
                InnerValue = ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_BEFORE_ABSOLUTE_TIME
            },
            AbsBefore = new Int64((long)TimePoint.InnerValue.InnerValue)
        };
    }
}