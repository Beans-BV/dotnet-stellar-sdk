using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Responses.Operations;
#nullable disable

/// <summary>
///     The response is for the genesis ledger of the Stellar network, and the links in the _links attribute provide links
///     to other relevant resources in Horizon.
///     The key of each link specifies that links relation to the current resource, and transactions means  “Transactions
///     that occurred in this operation”.
///     The transactions link is also templated, which means that the href attribute of the link is actually a URI
///     template, as specified by RFC 6570. We use URI templates
///     to show you what parameters a give resource can take.
/// </summary>
public class OperationResponseLinks
{
    /// <summary>
    ///     This endpoint represents effects that occurred as a result of a given operation.
    /// </summary>
    [JsonPropertyName("effects")]
    public Link<Page<EffectResponse>> Effects { get; init; }

    /// <summary>
    ///     This endpoint represents precedes that occurred as a result of a given operation.
    /// </summary>
    [JsonPropertyName("precedes")]
    public Link<OperationResponse> Precedes { get; init; }

    /// <summary>
    ///     This endpoint represents self that occurred as a result of a given operation.
    /// </summary>
    [JsonPropertyName("self")]
    public Link<OperationResponse> Self { get; init; }

    /// <summary>
    ///     This endpoint represents succeeds that occurred as a result of a given operation.
    /// </summary>
    [JsonPropertyName("succeeds")]
    public Link<OperationResponse> Succeeds { get; init; }

    /// <summary>
    ///     This endpoint represents transaction that occurred as a result of a given operation.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Link<TransactionResponse> Transaction { get; init; }
}