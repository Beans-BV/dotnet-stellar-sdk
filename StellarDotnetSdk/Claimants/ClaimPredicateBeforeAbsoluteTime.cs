using System;
using StellarDotnetSdk.Xdr;
using xdr_Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Claimants;

/// <summary>
///     Represents a claim predicate that is valid only before a specified absolute point in time.
///     The claimable balance can be claimed only before the given Unix timestamp.
/// </summary>
public class ClaimPredicateBeforeAbsoluteTime : ClaimPredicate
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClaimPredicateBeforeAbsoluteTime" /> class
    ///     with a <see cref="DateTimeOffset" /> representing the deadline.
    /// </summary>
    /// <param name="dateTime">The absolute point in time before which the balance can be claimed.</param>
    public ClaimPredicateBeforeAbsoluteTime(DateTimeOffset dateTime)
    {
        TimePoint.InnerValue = new Uint64((ulong)dateTime.ToUnixTimeSeconds());
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClaimPredicateBeforeAbsoluteTime" /> class
    ///     with an XDR <see cref="Xdr.TimePoint" /> value.
    /// </summary>
    /// <param name="timePoint">The XDR time point representing the deadline as a Unix timestamp.</param>
    public ClaimPredicateBeforeAbsoluteTime(TimePoint timePoint)
    {
        TimePoint = timePoint;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClaimPredicateBeforeAbsoluteTime" /> class
    ///     with a Unix timestamp in seconds.
    /// </summary>
    /// <param name="timePoint">The Unix timestamp (in seconds) before which the balance can be claimed.</param>
    public ClaimPredicateBeforeAbsoluteTime(ulong timePoint)
    {
        TimePoint = new TimePoint(new Uint64(timePoint));
    }

    /// <summary>
    ///     Gets the absolute deadline as a <see cref="DateTimeOffset" />, converted from the underlying Unix timestamp.
    /// </summary>
    public DateTimeOffset DateTime => DateTimeOffset.FromUnixTimeSeconds((long)TimePoint.InnerValue.InnerValue);

    /// <summary>Gets the XDR time point representing the absolute deadline as a Unix timestamp.</summary>
    public TimePoint TimePoint { get; } = new();

    /// <inheritdoc />
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