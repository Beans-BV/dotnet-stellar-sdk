using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents AccountMerge operation response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/operation.html
///     <seealso cref="Requests.OperationsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class BumpSequenceOperationResponse : OperationResponse
{
    public BumpSequenceOperationResponse()
    {
    }

    public BumpSequenceOperationResponse(long bumpTo)
    {
        BumpTo = bumpTo;
    }

    public override int TypeId => 11;

    [JsonProperty(PropertyName = "bump_to")]
    public long BumpTo { get; private set; }
}