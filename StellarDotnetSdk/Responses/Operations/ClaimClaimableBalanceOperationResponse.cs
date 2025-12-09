using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a claim_claimable_balance operation response.
///     Claims a claimable balance, transferring the balance to the claimant's account.
///     The claimant must be one of the claimants specified when the balance was created,
///     and their predicate conditions must be met.
/// </summary>
public class ClaimClaimableBalanceOperationResponse : OperationResponse
{
    public override int TypeId => 15;

    /// <summary>
    ///     The unique identifier of the claimable balance being claimed.
    /// </summary>
    [JsonPropertyName("balance_id")]
    public required string BalanceId { get; init; }

    /// <summary>
    ///     The account address that claimed the balance.
    /// </summary>
    [JsonPropertyName("claimant")]
    public required string Claimant { get; init; }

    /// <summary>
    ///     The muxed account representation of the claimant, if applicable.
    /// </summary>
    [JsonPropertyName("claimant_muxed")]
    public string? ClaimantMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the claimant, if applicable.
    /// </summary>
    [JsonPropertyName("claimant_muxed_id")]
    public ulong? ClaimantMuxedId { get; init; }
}