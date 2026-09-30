using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0012.Responses;

/// <summary>
///     A piece of information the anchor has already received for a customer, as listed under
///     <c>provided_fields</c> in the <c>GET /customer</c> response. Carries the same description as
///     <see cref="GetCustomerInfoField" /> plus the validation state of the submitted value.
/// </summary>
public class GetCustomerInfoProvidedField : GetCustomerInfoField
{
    /// <summary>
    ///     Gets the validation status of the field, or <c>null</c> when the anchor does not expose which fields were
    ///     accepted or rejected.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonConverter(typeof(ProvidedFieldStatusJsonConverter))]
    public ProvidedFieldStatus? Status { get; init; }

    /// <summary>
    ///     Gets a human-readable description of why the field was <see cref="ProvidedFieldStatus.Rejected" />.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }
}
