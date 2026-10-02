using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0012.Responses;

/// <summary>
///     Status of a customer's KYC process, as returned in the <c>status</c> field of <c>GET /customer</c>, of
///     <c>PUT /customer/verification</c> and of the callback payload. Wire values are the exact literals SEP-0012
///     defines; any other value is rejected when the response is parsed.
/// </summary>
/// <remarks>
///     Members are numbered from 1, so <c>default(CustomerStatus)</c> is not a defined value and never reads as a
///     real one. The type-level converter (also registered on <see cref="Converters.JsonOptions.DefaultOptions" />)
///     serializes the exact wire literal and rejects any other value, including a bare ordinal, when the enum is
///     (de)serialized on its own — as a value or as a dictionary key — unless the caller's own options register an
///     enum converter that claims it first (an options converter outranks a type-level attribute). The SEP-0012
///     response properties are pinned with property-level converters, which hold regardless. Serializing an object
///     whose <see cref="CustomerStatus" /> property was left at <c>default</c> fails with a
///     <see cref="System.Text.Json.JsonException" />.
/// </remarks>
[JsonConverter(typeof(CustomerStatusJsonConverter))]
public enum CustomerStatus
{
    /// <summary>
    ///     <c>ACCEPTED</c>: all required KYC fields have been accepted and the customer has been validated for the
    ///     requested <c>type</c>. An accepted customer may later move back to another status.
    /// </summary>
    Accepted = 1,

    /// <summary>
    ///     <c>PROCESSING</c>: the KYC process is in flight; check again later to see whether more information is
    ///     needed.
    /// </summary>
    Processing,

    /// <summary>
    ///     <c>NEEDS_INFO</c>: more information must be provided to finish KYC. The response's
    ///     <see cref="GetCustomerInfoResponse.Fields" /> lists what is still required.
    /// </summary>
    NeedsInfo,

    /// <summary>
    ///     <c>REJECTED</c>: KYC has failed and will never succeed. The response's
    ///     <see cref="GetCustomerInfoResponse.Message" /> explains why.
    /// </summary>
    Rejected,
}
