using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;
#nullable disable

/// <inheritdoc />
/// <summary>
///     Represents ClawbackClaimableBalanceOperation response.
/// </summary>
public class ClawbackClaimableBalanceOperationResponse : OperationResponse
{
    public override int TypeId => 20;

    /// <summary>
    ///     Claimable balance identifier of the claimable balance to be clawed back.
    /// </summary>
    [JsonPropertyName("balance_id")]
    public string BalanceId { get; init; }
}