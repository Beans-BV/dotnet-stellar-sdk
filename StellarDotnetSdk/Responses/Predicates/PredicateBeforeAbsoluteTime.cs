using System;
using System.Globalization;
using ClaimPredicate = StellarDotnetSdk.Claimants.ClaimPredicate;

namespace StellarDotnetSdk.Responses.Predicates;

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
    /// <remarks>
    ///     When no epoch is available, <see cref="AbsBefore" /> is parsed with the invariant culture and a
    ///     value without an offset designator is interpreted as UTC — the same rules the
    ///     <see cref="Converters.PredicateJsonConverter" /> consistency check applies — so the deadline
    ///     resolves to the same instant regardless of the machine's culture or local time zone.
    /// </remarks>
    public DateTimeOffset DateTime => AbsBeforeEpoch.HasValue
        ? DateTimeOffset.FromUnixTimeSeconds(AbsBeforeEpoch.Value)
        : DateTimeOffset.Parse(AbsBefore, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    /// <inheritdoc />
    public override ClaimPredicate ToClaimPredicate()
    {
        return ClaimPredicate.BeforeAbsoluteTime(DateTime);
    }
}