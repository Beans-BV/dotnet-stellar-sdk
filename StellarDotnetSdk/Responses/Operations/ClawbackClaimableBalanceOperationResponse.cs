using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a clawback_claimable_balance operation response.
///     Claws back (destroys) a claimable balance, removing it from the network.
///     This operation can only be performed by the asset issuer on claimable balances containing assets
///     that have the clawback-enabled flag set. The claimable balance and its assets are permanently removed.
/// </summary>
public class ClawbackClaimableBalanceOperationResponse : OperationResponse
{
    public override int TypeId => 20;

    /// <summary>
    ///     The unique identifier of the claimable balance being clawed back.
    /// </summary>
    [JsonPropertyName("balance_id")]
    public required string BalanceId { get; init; }
}