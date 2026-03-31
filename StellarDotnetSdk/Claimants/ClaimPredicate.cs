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
    /// <summary>Converts this claim predicate to its XDR representation.</summary>
    /// <returns>An XDR <see cref="Xdr.ClaimPredicate" /> object.</returns>
    public abstract Xdr.ClaimPredicate ToXdr();

    /// <summary>Creates a predicate requiring both sub-predicates to be satisfied.</summary>
    /// <param name="leftPredicate">The first predicate.</param>
    /// <param name="rightPredicate">The second predicate.</param>
    /// <returns>A new <see cref="ClaimPredicateOr" /> instance.</returns>
    public static ClaimPredicate Or(ClaimPredicate leftPredicate, ClaimPredicate rightPredicate)
    {
        return new ClaimPredicateOr(leftPredicate, rightPredicate);
    }

    /// <summary>Creates a predicate requiring both sub-predicates to be satisfied.</summary>
    /// <param name="leftPredicate">The first predicate.</param>
    /// <param name="rightPredicate">The second predicate.</param>
    /// <returns>A new <see cref="ClaimPredicateAnd" /> instance.</returns>
    public static ClaimPredicate And(ClaimPredicate leftPredicate, ClaimPredicate rightPredicate)
    {
        return new ClaimPredicateAnd(leftPredicate, rightPredicate);
    }

    /// <summary>Creates a predicate that negates the given predicate.</summary>
    /// <param name="predicate">The predicate to negate.</param>
    /// <returns>A new <see cref="ClaimPredicateNot" /> instance.</returns>
    public static ClaimPredicate Not(ClaimPredicate predicate)
    {
        return new ClaimPredicateNot(predicate);
    }

    /// <summary>Creates an unconditional predicate that is always satisfied.</summary>
    /// <returns>A new <see cref="ClaimPredicateUnconditional" /> instance.</returns>
    public static ClaimPredicate Unconditional()
    {
        return new ClaimPredicateUnconditional();
    }

    /// <summary>Creates a predicate valid only before the given absolute time point.</summary>
    /// <param name="timePoint">The XDR time point representing the deadline.</param>
    /// <returns>A new <see cref="ClaimPredicateBeforeAbsoluteTime" /> instance.</returns>
    public static ClaimPredicate BeforeAbsoluteTime(TimePoint timePoint)
    {
        return new ClaimPredicateBeforeAbsoluteTime(timePoint);
    }

    /// <summary>Creates a predicate valid only before the given Unix timestamp.</summary>
    /// <param name="timePoint">The Unix timestamp in seconds.</param>
    /// <returns>A new <see cref="ClaimPredicateBeforeAbsoluteTime" /> instance.</returns>
    public static ClaimPredicate BeforeAbsoluteTime(ulong timePoint)
    {
        return new ClaimPredicateBeforeAbsoluteTime(timePoint);
    }

    /// <summary>Creates a predicate valid only before the given date and time.</summary>
    /// <param name="dateTime">The absolute deadline.</param>
    /// <returns>A new <see cref="ClaimPredicateBeforeAbsoluteTime" /> instance.</returns>
    public static ClaimPredicate BeforeAbsoluteTime(DateTimeOffset dateTime)
    {
        return new ClaimPredicateBeforeAbsoluteTime(dateTime);
    }

    /// <summary>Creates a predicate valid only before the given relative duration has elapsed.</summary>
    /// <param name="duration">The XDR duration in seconds.</param>
    /// <returns>A new <see cref="ClaimPredicateBeforeRelativeTime" /> instance.</returns>
    public static ClaimPredicate BeforeRelativeTime(Duration duration)
    {
        return new ClaimPredicateBeforeRelativeTime(duration);
    }

    /// <summary>Creates a predicate valid only before the given number of seconds has elapsed.</summary>
    /// <param name="duration">The duration in seconds.</param>
    /// <returns>A new <see cref="ClaimPredicateBeforeRelativeTime" /> instance.</returns>
    public static ClaimPredicate BeforeRelativeTime(ulong duration)
    {
        return new ClaimPredicateBeforeRelativeTime(duration);
    }

    /// <summary>
    ///     Creates a <see cref="ClaimPredicate" /> from an XDR claim predicate, dispatching to the correct subclass.
    /// </summary>
    /// <param name="xdr">The XDR claim predicate to convert.</param>
    /// <returns>A concrete <see cref="ClaimPredicate" /> subclass instance.</returns>
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