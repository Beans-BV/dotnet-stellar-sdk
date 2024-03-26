using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents AccountMerge operation response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/operation.html
///     <seealso cref="Requests.OperationsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
[JsonObject]
public class BeginSponsoringFutureReservesOperationResponse : OperationResponse
{
    public BeginSponsoringFutureReservesOperationResponse()
    {
    }


    public BeginSponsoringFutureReservesOperationResponse(string sponsoredID)
    {
        SponsoredID = sponsoredID;
    }

    public override int TypeId => 16;

    [JsonProperty(PropertyName = "sponsored_id")]
    public string SponsoredID { get; private set; }
}