using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;
using ClaimPredicate = StellarDotnetSdk.Claimants.ClaimPredicate;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Base class for claim predicates in Horizon API responses.
/// </summary>
/// <remarks>
///     <para>
///         This abstract class and its implementations are specifically designed for deserializing
///         predicate data from Horizon API responses.
///     </para>
///     <para>
///         <strong>For building transactions</strong>, use <see cref="Claimants.ClaimPredicate" /> instead.
///         You can convert this response predicate to a transaction predicate using <see cref="ToClaimPredicate" />.
///     </para>
/// </remarks>
/// <seealso cref="Claimants.ClaimPredicate" />
[JsonConverter(typeof(PredicateJsonConverter))]
public abstract class Predicate
{
    /// <summary>
    ///     Converts this response predicate to a ClaimPredicate for use in transactions.
    /// </summary>
    /// <returns>A ClaimPredicate representing this predicate.</returns>
    public abstract ClaimPredicate ToClaimPredicate();
}