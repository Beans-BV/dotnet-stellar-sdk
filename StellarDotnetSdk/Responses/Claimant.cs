using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;
#nullable disable

/// <summary>
///     Represents a claimant in Horizon API responses.
/// </summary>
/// <remarks>
///     <para>
///         This class is specifically designed for deserializing claimant data from Horizon API responses,
///         such as when querying claimable balances.
///     </para>
///     <para>
///         <strong>For building transactions</strong>, use <see cref="Claimants.Claimant"/> instead.
///         You can convert the <see cref="Predicate"/> to a transaction predicate using
///         <see cref="Predicate.ToClaimPredicate"/>.
///     </para>
/// </remarks>
/// <seealso cref="Claimants.Claimant"/>
public class Claimant
{
    [JsonPropertyName("destination")]
    public string Destination { get; init; }

    [JsonPropertyName("predicate")]
    public Predicate Predicate { get; init; }
}