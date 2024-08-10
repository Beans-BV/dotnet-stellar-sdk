using System;
using StellarDotnetSdk.Xdr;
using xdr_Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Claimants;

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

    public override Xdr.ClaimPredicate ToXdr()
    {
        return new Xdr.ClaimPredicate
        {
            Discriminant = new ClaimPredicateType
            {
                InnerValue = ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_BEFORE_ABSOLUTE_TIME,
            },
            AbsBefore = new xdr_Int64((long)TimePoint.InnerValue.InnerValue),
        };
    }
}