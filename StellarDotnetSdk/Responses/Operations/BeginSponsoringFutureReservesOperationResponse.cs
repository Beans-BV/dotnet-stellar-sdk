using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents AccountMerge operation response.
/// </summary>
public class BeginSponsoringFutureReservesOperationResponse : OperationResponse
{
    public override int TypeId => 16;

    [JsonPropertyName("sponsored_id")] public string SponsoredId { get; init; }
}