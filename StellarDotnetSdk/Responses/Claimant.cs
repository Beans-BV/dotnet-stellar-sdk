using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Predicates;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a claimant in Horizon API responses.
/// </summary>
/// <remarks>
///     <para>
///         This class is specifically designed for deserializing claimant data from Horizon API responses,
///         such as when querying claimable balances.
///     </para>
///     <para>
///         <strong>For building transactions</strong>, use <see cref="Claimants.Claimant" /> instead.
///         You can convert the <see cref="Predicate" /> to a transaction predicate using
///         <see cref="Predicates.Predicate.ToClaimPredicate" />.
///     </para>
/// </remarks>
/// <seealso cref="Claimants.Claimant" />
public class Claimant
{
    /// <summary>
    ///     The account ID that can claim this balance when the predicate conditions are met.
    /// </summary>
    [JsonPropertyName("destination")]
    public required string Destination { get; init; }

    /// <summary>
    ///     The conditions that must be satisfied for this claimant to claim the balance.
    /// </summary>
    [JsonPropertyName("predicate")]
    public required Predicate Predicate { get; init; }
}