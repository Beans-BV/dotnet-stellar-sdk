using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents AccountMerge operation response.
/// </summary>
public class BeginSponsoringFutureReservesOperationResponse : OperationResponse
{
    public override int TypeId => 16;

    [JsonProperty(PropertyName = "sponsored_id")]
    public string SponsoredId { get; init; }
}