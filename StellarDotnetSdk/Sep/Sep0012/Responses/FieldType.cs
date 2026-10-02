using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0012.Responses;

/// <summary>
///     Data type of a KYC field value, as returned in the <c>type</c> property of
///     <see cref="GetCustomerInfoField" /> and <see cref="GetCustomerInfoProvidedField" />. Wire values are the exact
///     lowercase literals SEP-0012 defines; any other value is rejected when the response is parsed.
/// </summary>
/// <remarks>
///     Members are numbered from 1, so <c>default(FieldType)</c> is not a defined value and never reads as a
///     real one. The type-level converter (also registered on <see cref="Converters.JsonOptions.DefaultOptions" />)
///     serializes the exact wire literal and rejects any other value, including a bare ordinal, when the enum is
///     (de)serialized on its own — as a value or as a dictionary key — unless the caller's own options register an
///     enum converter that claims it first (an options converter outranks a type-level attribute). The SEP-0012
///     response properties are pinned with property-level converters, which hold regardless. Serializing an object
///     whose <see cref="FieldType" /> property was left at <c>default</c> fails with a
///     <see cref="System.Text.Json.JsonException" />.
/// </remarks>
[JsonConverter(typeof(FieldTypeJsonConverter))]
public enum FieldType
{
    /// <summary><c>string</c>: a text value.</summary>
    String = 1,

    /// <summary>
    ///     <c>binary</c>: binary data such as a photo or document. Submit it as a file through
    ///     <see cref="Requests.PutCustomerInfoRequest" /> (sent as multipart/form-data after all text fields) or
    ///     upload it with <c>POST /customer/files</c> and reference the returned <c>file_id</c>.
    /// </summary>
    Binary,

    /// <summary><c>number</c>: a numeric value.</summary>
    Number,

    /// <summary><c>date</c>: a date value (ISO 8601).</summary>
    Date,
}
