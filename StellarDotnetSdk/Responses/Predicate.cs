using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Xdr;
using claimant_ClaimPredicate = StellarDotnetSdk.Claimants.ClaimPredicate;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a claim predicate that defines conditions for claiming a claimable balance.
///     Predicates can be combined using logical operators (AND, OR, NOT) or specify time-based conditions.
/// </summary>
public sealed class Predicate
{
    /// <summary>
    ///     An array of predicates that must all be true (logical AND).
    ///     Typically contains exactly two predicates.
    /// </summary>
    [JsonPropertyName("and")]
    public Predicate[]? And { get; init; }

    /// <summary>
    ///     An array of predicates where at least one must be true (logical OR).
    ///     Typically contains exactly two predicates.
    /// </summary>
    [JsonPropertyName("or")]
    public Predicate[]? Or { get; init; }

    /// <summary>
    ///     A predicate that must be false for this predicate to be true (logical NOT).
    /// </summary>
    [JsonPropertyName("not")]
    public Predicate? Not { get; init; }

    /// <summary>
    ///     When true, the predicate is always satisfied (no conditions).
    /// </summary>
    [JsonPropertyName("unconditional")]
    public bool? Unconditional { get; init; }

    /// <summary>
    ///     Deadline for when the balance must be claimed.
    ///     If a balance is claimed before the date then the clause of the condition is satisfied.
    /// </summary>
    [JsonPropertyName("abs_before")]
    public string? AbsBefore { get; init; }

    /// <summary>
    ///     A UNIX epoch value in seconds representing the same deadline date as abs_before.
    /// </summary>
    [JsonPropertyName("abs_before_epoch")]
    public long? AbsBeforeEpoch { get; init; }

    /// <summary>
    ///     A relative time in seconds from the claimable balance creation time.
    ///     The balance can only be claimed within this many seconds of its creation.
    /// </summary>
    [JsonPropertyName("rel_before")]
    public long? RelBefore { get; init; }

    /// <summary>
    ///     Converts this response predicate to a ClaimPredicate object for use in transactions.
    /// </summary>
    /// <returns>A ClaimPredicate representing this predicate.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the predicate structure is invalid.</exception>
    public claimant_ClaimPredicate ToClaimPredicate()
    {
        if (And is { Length: > 0 })
        {
            var leftPredicate = And[0].ToClaimPredicate();
            var rightPredicate = And[1].ToClaimPredicate();
            return claimant_ClaimPredicate.And(leftPredicate, rightPredicate);
        }

        if (Or is { Length: > 0 })
        {
            var leftPredicate = Or[0].ToClaimPredicate();
            var rightPredicate = Or[1].ToClaimPredicate();
            return claimant_ClaimPredicate.Or(leftPredicate, rightPredicate);
        }

        if (Not != null)
        {
            var predicate = Not.ToClaimPredicate();
            return claimant_ClaimPredicate.Not(predicate);
        }

        if (Unconditional ?? false)
        {
            return claimant_ClaimPredicate.Unconditional();
        }

        if (AbsBeforeEpoch != null)
        {
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(AbsBeforeEpoch.Value);
            return claimant_ClaimPredicate.BeforeAbsoluteTime(dateTime);
        }
        
        if (AbsBefore != null)
        {
            var dateTime = DateTimeOffset.Parse(AbsBefore);
            return claimant_ClaimPredicate.BeforeAbsoluteTime(dateTime);
        }

        if (RelBefore != null)
        {
            return claimant_ClaimPredicate.BeforeRelativeTime(new Duration(new Uint64((ulong)RelBefore.Value)));
        }

        throw new InvalidOperationException("Invalid Predicate: no valid predicate conditions found.");
    }
}