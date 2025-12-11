using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents an end_sponsoring_future_reserves operation response.
///     This operation terminates the current sponsorship relationship established by
///     a begin_sponsoring_future_reserves operation. It must be submitted by the
///     sponsored account to complete the sponsorship sandwich.
/// </summary>
public class EndSponsoringFutureReservesOperationResponse : OperationResponse
{
    public override int TypeId => 17;

    /// <summary>
    ///     The account that initiated the sponsorship with begin_sponsoring_future_reserves.
    /// </summary>
    [JsonPropertyName("begin_sponsor")]
    public required string BeginSponsor { get; init; }

    /// <summary>
    ///     The muxed account representation of the sponsoring account, if applicable.
    /// </summary>
    [JsonPropertyName("begin_sponsor_muxed")]
    public string? BeginSponsorMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the sponsoring account, if applicable.
    /// </summary>
    [JsonPropertyName("begin_sponsor_muxed_id")]
    public ulong? BeginSponsorMuxedId { get; init; }
}