using ClaimPredicate = StellarDotnetSdk.Claimants.ClaimPredicate;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a predicate that is satisfied when the elapsed time since creation is less than a relative duration.
/// </summary>
public sealed class PredicateBeforeRelativeTime : Predicate
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PredicateBeforeRelativeTime" /> class.
    /// </summary>
    /// <param name="relBefore">The relative time in seconds.</param>
    public PredicateBeforeRelativeTime(long relBefore)
    {
        RelBefore = relBefore;
    }

    /// <summary>
    ///     A relative time in seconds from the claimable balance creation time.
    ///     The balance can only be claimed within this many seconds of its creation.
    /// </summary>
    public long RelBefore { get; }

    /// <inheritdoc />
    public override ClaimPredicate ToClaimPredicate()
    {
        return ClaimPredicate.BeforeRelativeTime((ulong)RelBefore);
    }
}