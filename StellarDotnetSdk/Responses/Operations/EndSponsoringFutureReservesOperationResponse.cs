using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents AccountMerge operation response.
/// </summary>
public class EndSponsoringFutureReservesOperationResponse : OperationResponse
{
    public override int TypeId => 17;

    [JsonProperty(PropertyName = "begin_sponsor")]
    public string BeginSponsor { get; init; }

    [JsonProperty(PropertyName = "begin_sponsor_muxed")]
    public string BeginSponsorMuxed { get; init; }

    [JsonProperty(PropertyName = "begin_sponsor_muxed_id")]
    public ulong? BeginSponsorMuxedID { get; init; }
}