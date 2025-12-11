using System;
using StellarDotnetSdk.Responses.Predicates;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Claimants;

/// <summary>
///     Represents a claim predicate for building claimable balance operations in Stellar transactions.
/// </summary>
/// <remarks>
///     <para>
///         This abstract class and its implementations are used when constructing
///         <see cref="StellarDotnetSdk.Operations.CreateClaimableBalanceOperation" /> to define
///         conditions under which a claimable balance can be claimed.
///     </para>
///     <para>
///         Predicates can be combined using logical operators:
///         <list type="bullet">
///             <item><see cref="And" /> - Both predicates must be true</item>
///             <item><see cref="Or" /> - At least one predicate must be true</item>
///             <item><see cref="Not" /> - The predicate must be false</item>
///         </list>
///     </para>
///     <para>
///         <strong>For deserializing Horizon API responses</strong>, use <see cref="Predicate" /> instead,
///         which can be converted to this type using <see cref="Predicate.ToClaimPredicate" />.
///     </para>
/// </remarks>
/// <seealso cref="Predicate" />
public abstract class ClaimPredicate
{
    public abstract Xdr.ClaimPredicate ToXdr();

    public static ClaimPredicate Or(ClaimPredicate leftPredicate, ClaimPredicate rightPredicate)
    {
        return new ClaimPredicateOr(leftPredicate, rightPredicate);
    }

    public static ClaimPredicate And(ClaimPredicate leftPredicate, ClaimPredicate rightPredicate)
    {
        return new ClaimPredicateAnd(leftPredicate, rightPredicate);
    }

    public static ClaimPredicate Not(ClaimPredicate predicate)
    {
        return new ClaimPredicateNot(predicate);
    }

    public static ClaimPredicate Unconditional()
    {
        return new ClaimPredicateUnconditional();
    }

    public static ClaimPredicate BeforeAbsoluteTime(TimePoint timePoint)
    {
        return new ClaimPredicateBeforeAbsoluteTime(timePoint);
    }

    public static ClaimPredicate BeforeAbsoluteTime(ulong timePoint)
    {
        return new ClaimPredicateBeforeAbsoluteTime(timePoint);
    }

    public static ClaimPredicate BeforeAbsoluteTime(DateTimeOffset dateTime)
    {
        return new ClaimPredicateBeforeAbsoluteTime(dateTime);
    }

    public static ClaimPredicate BeforeRelativeTime(Duration duration)
    {
        return new ClaimPredicateBeforeRelativeTime(duration);
    }

    public static ClaimPredicate BeforeRelativeTime(ulong duration)
    {
        return new ClaimPredicateBeforeRelativeTime(duration);
    }

    public static ClaimPredicate FromXdr(Xdr.ClaimPredicate xdr)
    {
        switch (xdr.Discriminant.InnerValue)
        {
            case ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_OR:
                if (xdr.OrPredicates.Length != 2)
                {
                    throw new Exception("ClaimPredicate.OrPredicates expected to have length 2");
                }
                return Or(FromXdr(xdr.OrPredicates[0]), FromXdr(xdr.OrPredicates[1]));
            case ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_AND:
                if (xdr.AndPredicates.Length != 2)
                {
                    throw new Exception("ClaimPredicate.AndPredicates expected to have length 2");
                }
                return And(FromXdr(xdr.AndPredicates[0]), FromXdr(xdr.AndPredicates[1]));
            case ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_NOT:
                if (xdr.NotPredicate == null)
                {
                    throw new Exception("ClaimPredicate.NotPredicate expected to be not null");
                }
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