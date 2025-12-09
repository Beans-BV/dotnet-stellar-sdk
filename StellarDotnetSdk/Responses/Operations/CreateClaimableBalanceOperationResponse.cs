using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a create_claimable_balance operation response.
///     Creates a claimable balance that can be claimed by one or more claimants according to specified predicates.
///     The balance is held by the network and can be claimed when the predicate conditions are met.
/// </summary>
public class CreateClaimableBalanceOperationResponse : OperationResponse
{
    public override int TypeId => 14;

    /// <summary>
    ///     The account that sponsored the creation of the claimable balance.
    /// </summary>
    [JsonPropertyName("sponsor")]
    public required string Sponsor { get; init; }

    /// <summary>
    ///     The asset held in the claimable balance.
    ///     For native XLM, this will be "native".
    ///     For issued assets, this will be in the format "CODE:ISSUER".
    /// </summary>
    [JsonPropertyName("asset")]
    public required string Asset { get; init; }

    /// <summary>
    ///     The amount of the asset held in the claimable balance.
    /// </summary>
    [JsonPropertyName("amount")]
    public required string Amount { get; init; }

    /// <summary>
    ///     The list of claimants who can claim this balance and their predicates.
    ///     Each claimant has a destination account and a predicate that must be satisfied to claim the balance.
    /// </summary>
    [JsonPropertyName("claimants")]
    public required Claimant[] Claimants { get; init; }
}