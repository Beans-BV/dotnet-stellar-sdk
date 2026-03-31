using System;
using StellarDotnetSdk.Xdr;
using xdr_Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Claimants;

/// <summary>
///     Represents a claim predicate that is valid only before a specified duration has elapsed
///     since the claimable balance was created. The duration is measured in seconds.
/// </summary>
public class ClaimPredicateBeforeRelativeTime : ClaimPredicate
{
    /// <summary>
    ///     Initializes a new instance from an XDR <see cref="Xdr.Duration" /> value.
    /// </summary>
    /// <param name="duration">The XDR duration representing the relative time window in seconds.</param>
    [Obsolete("Use the ClaimPredicateBeforeRelativeTime(long duration) constructor instead.")]
    public ClaimPredicateBeforeRelativeTime(Duration duration)
    {
        Duration = (long)duration.InnerValue.InnerValue;
    }

    /// <summary>
    ///     Initializes a new instance with a duration in seconds as an unsigned 64-bit integer.
    /// </summary>
    /// <param name="duration">The duration in seconds.</param>
    [Obsolete("Use the ClaimPredicateBeforeRelativeTime(long duration) constructor instead.")]
    public ClaimPredicateBeforeRelativeTime(ulong duration)
    {
        Duration = (long)duration;
    }

    /// <summary>
    ///     Initializes a new instance with a duration in seconds.
    /// </summary>
    /// <param name="duration">The duration in seconds before which the balance can be claimed.</param>
    public ClaimPredicateBeforeRelativeTime(long duration)
    {
        Duration = duration;
    }

    /// <summary>Gets the relative duration in seconds before which the balance can be claimed.</summary>
    public long Duration { get; }

    /// <inheritdoc />
    public override Xdr.ClaimPredicate ToXdr()
    {
        return new Xdr.ClaimPredicate
        {
            Discriminant = new ClaimPredicateType
            {
                InnerValue = ClaimPredicateType.ClaimPredicateTypeEnum.CLAIM_PREDICATE_BEFORE_RELATIVE_TIME,
            },
            RelBefore = new xdr_Int64(Duration),
        };
    }
}