using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk
{
    public abstract class ClaimPredicate
    {
        public abstract xdr.ClaimPredicate ToXdr();

        public static ClaimPredicate Or(ClaimPredicate leftPredicate, ClaimPredicate rightPredicate) => new ClaimPredicateOr(leftPredicate, rightPredicate);

        public static ClaimPredicate And(ClaimPredicate leftPredicate, ClaimPredicate rightPredicate) => new ClaimPredicateAnd(leftPredicate, rightPredicate);

        public static ClaimPredicate Not(ClaimPredicate predicate) => new ClaimPredicateNot(predicate);

        public static ClaimPredicate Unconditional() => new ClaimPredicateUnconditional();

        public static ClaimPredicate BeforeAbsoluteTime(TimePoint timePoint) => new ClaimPredicateBeforeAbsoluteTime(timePoint);
        
        public static ClaimPredicate BeforeAbsoluteTime(ulong timePoint) => new ClaimPredicateBeforeAbsoluteTime(timePoint);

        public static ClaimPredicate BeforeAbsoluteTime(DateTimeOffset dateTime) => new ClaimPredicateBeforeAbsoluteTime(dateTime);

        public static ClaimPredicate BeforeRelativeTime(Duration duration) => new ClaimPredicateBeforeRelativeTime(duration);
        
        public static ClaimPredicate BeforeRelativeTime(ulong duration) => new ClaimPredicateBeforeRelativeTime(duration);

        public static ClaimPredicate FromXdr(xdr.ClaimPredicate xdr)
        {
            switch (xdr.Discriminant.InnerValue)
            {
                case ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_OR:
                    if (xdr.OrPredicates.Length != 2)
                        throw new Exception("ClaimPredicate.OrPredicates expected to have length 2");
                    return Or(FromXdr(xdr.OrPredicates[0]), ClaimPredicate.FromXdr(xdr.OrPredicates[1]));
                case ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_AND:
                    if (xdr.AndPredicates.Length != 2)
                        throw new Exception("ClaimPredicate.AndPredicates expected to have length 2");
                    return And(FromXdr(xdr.AndPredicates[0]), ClaimPredicate.FromXdr(xdr.AndPredicates[1]));
                case ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_NOT:
                    if (xdr.NotPredicate == null)
                        throw new Exception("ClaimPredicate.NotPredicate expected to be not null");
                    return Not(FromXdr(xdr.NotPredicate));
                case ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_UNCONDITIONAL:
                    return Unconditional();
                case ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_BEFORE_ABSOLUTE_TIME:
                    return BeforeAbsoluteTime(new TimePoint(new Uint64((ulong)xdr.AbsBefore.InnerValue)));
                case ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_BEFORE_RELATIVE_TIME:
                    return BeforeRelativeTime(new Duration(new Uint64((ulong)xdr.RelBefore.InnerValue)));
                default:
                    throw new Exception("Unknown claim predicate " + xdr.Discriminant.InnerValue);
            }
        }
    }
}