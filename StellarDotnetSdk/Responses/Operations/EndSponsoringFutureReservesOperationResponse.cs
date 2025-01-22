using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents AccountMerge operation response.
/// </summary>
public class EndSponsoringFutureReservesOperationResponse : OperationResponse
{
    public override int TypeId => 17;

    [JsonPropertyName("begin_sponsor")]
    public string BeginSponsor { get; init; }

    [JsonPropertyName("begin_sponsor_muxed")]
    public string BeginSponsorMuxed { get; init; }

    [JsonPropertyName("begin_sponsor_muxed_id")]
    public ulong? BeginSponsorMuxedID { get; init; }
}