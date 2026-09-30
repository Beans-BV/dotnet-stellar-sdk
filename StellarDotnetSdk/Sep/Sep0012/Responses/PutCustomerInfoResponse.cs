using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0012.Responses;

/// <summary>
///     Response of <c>PUT /customer</c>: the identifier of the created or updated customer.
/// </summary>
public sealed class PutCustomerInfoResponse
{
    /// <summary>
    ///     Gets the identifier of the created or updated customer. Use it in later requests to check the customer's
    ///     status or update their information; other SEPs may also use it to identify the customer.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("id")]
    public string Id { get; init; } = null!;
}
