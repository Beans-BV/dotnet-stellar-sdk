using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a begin_sponsoring_future_reserves operation response.
///     This operation establishes a sponsorship relationship where the source account
///     sponsors the reserves (minimum balance requirements) for operations performed
///     by another account until an end_sponsoring_future_reserves operation is encountered.
/// </summary>
public class BeginSponsoringFutureReservesOperationResponse : OperationResponse
{
    public override int TypeId => 16;

    /// <summary>
    ///     The account ID that will have its reserves sponsored by the source account.
    /// </summary>
    [JsonPropertyName("sponsored_id")]
    public required string SponsoredId { get; init; }
}