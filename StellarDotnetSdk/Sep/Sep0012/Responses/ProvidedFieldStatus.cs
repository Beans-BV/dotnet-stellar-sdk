using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0012.Responses;

/// <summary>
///     Status of a single field the anchor has already received, as returned in
///     <see cref="GetCustomerInfoProvidedField.Status" />. Wire values are the exact literals SEP-0012 defines; any
///     other value is rejected when the response is parsed.
/// </summary>
/// <remarks>
///     Members are numbered from 1, so <c>default(ProvidedFieldStatus)</c> is not a defined value and never reads as a
///     real one. The type-level converter (also registered on <see cref="Converters.JsonOptions.DefaultOptions" />)
///     serializes the exact wire literal and rejects any other value, including a bare ordinal, when the enum is
///     (de)serialized on its own — as a value or as a dictionary key — unless the caller's own options register an
///     enum converter that claims it first (an options converter outranks a type-level attribute). The SEP-0012
///     response properties are pinned with property-level converters, which hold regardless. Serializing an object
///     whose <see cref="ProvidedFieldStatus" /> property was left at <c>default</c> fails with a
///     <see cref="System.Text.Json.JsonException" />.
/// </remarks>
[JsonConverter(typeof(ProvidedFieldStatusJsonConverter))]
public enum ProvidedFieldStatus
{
    /// <summary><c>ACCEPTED</c>: the field has been validated.</summary>
    Accepted = 1,

    /// <summary>
    ///     <c>PROCESSING</c>: the field is being validated; poll <c>GET /customer</c> later for the outcome.
    /// </summary>
    Processing,

    /// <summary>
    ///     <c>REJECTED</c>: the field did not pass validation. <see cref="GetCustomerInfoProvidedField.Error" />
    ///     describes why. If the value may be resubmitted the customer status is <c>NEEDS_INFO</c>, otherwise
    ///     <c>REJECTED</c>.
    /// </summary>
    Rejected,

    /// <summary>
    ///     <c>VERIFICATION_REQUIRED</c>: the field must be verified, for example with a confirmation code sent to
    ///     the customer. Submit it through <see cref="Requests.PutCustomerInfoRequest.VerificationFields" />.
    /// </summary>
    VerificationRequired,
}
