using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents AccountMerge operation response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/operation.html
///     <seealso cref="Requests.OperationsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class EndSponsoringFutureReservesOperationResponse : OperationResponse
{
    public EndSponsoringFutureReservesOperationResponse()
    {
    }


    public EndSponsoringFutureReservesOperationResponse(string beginSponsor)
    {
        BeginSponsor = beginSponsor;
    }

    public override int TypeId => 17;

    [JsonProperty(PropertyName = "begin_sponsor")]
    public string BeginSponsor { get; private set; }

    [JsonProperty(PropertyName = "begin_sponsor_muxed")]
    public string BeginSponsorMuxed { get; private set; }

    [JsonProperty(PropertyName = "begin_sponsor_muxed_id")]
    public ulong? BeginSponsorMuxedID { get; private set; }
}