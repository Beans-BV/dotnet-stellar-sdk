using System;
using ClaimPredicate = StellarDotnetSdk.Claimants.ClaimPredicate;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a predicate that is satisfied when the current time is before an absolute deadline.
/// </summary>
public sealed class PredicateBeforeAbsoluteTime : Predicate
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PredicateBeforeAbsoluteTime" /> class.
    /// </summary>
    /// <param name="absBefore">The deadline as an ISO 8601 formatted string.</param>
    /// <param name="absBeforeEpoch">The deadline as a UNIX epoch value in seconds (optional).</param>
    public PredicateBeforeAbsoluteTime(string absBefore, long? absBeforeEpoch = null)
    {
        AbsBefore = absBefore;
        AbsBeforeEpoch = absBeforeEpoch;
    }

    /// <summary>
    ///     Deadline for when the balance must be claimed, as an ISO 8601 formatted string.
    ///     If a balance is claimed before the date then the clause of the condition is satisfied.
    /// </summary>
    public string AbsBefore { get; }

    /// <summary>
    ///     A UNIX epoch value in seconds representing the same deadline date as <see cref="AbsBefore" />.
    /// </summary>
    public long? AbsBeforeEpoch { get; }

    /// <summary>
    ///     Gets the deadline as a <see cref="DateTimeOffset" />.
    /// </summary>
    public DateTimeOffset DateTime => AbsBeforeEpoch.HasValue
        ? DateTimeOffset.FromUnixTimeSeconds(AbsBeforeEpoch.Value)
        : DateTimeOffset.Parse(AbsBefore);

    /// <inheritdoc />
    public override ClaimPredicate ToClaimPredicate()
    {
        return ClaimPredicate.BeforeAbsoluteTime(DateTime);
    }
}