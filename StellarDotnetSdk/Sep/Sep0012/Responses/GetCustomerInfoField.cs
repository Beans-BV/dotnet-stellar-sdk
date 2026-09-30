using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0012.Responses;

/// <summary>
///     A piece of information the anchor has not yet received for a customer, as listed under <c>fields</c> in the
///     <c>GET /customer</c> response. The dictionary key it is stored under is the field name, normally a SEP-0009
///     field such as <c>first_name</c> or <c>photo_id_front</c>.
/// </summary>
public class GetCustomerInfoField
{
    /// <summary>
    ///     Gets the data type of the field value: <c>string</c>, <c>binary</c>, <c>number</c>, or <c>date</c>.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(FieldTypeJsonConverter))]
    public FieldType Type { get; init; }

    /// <summary>
    ///     Gets a human-readable description of the field, especially important when it is not a SEP-0009 field.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    ///     Gets the list of valid values for the field, or <c>null</c> when any value is accepted. A numeric choice is
    ///     kept as its JSON text, for example <c>"1"</c>.
    /// </summary>
    [JsonPropertyName("choices")]
    [JsonConverter(typeof(ChoicesJsonConverter))]
    public string[]? Choices { get; init; }

    /// <summary>
    ///     Gets whether the field may be omitted. SEP-0012 defines an absent value as <c>false</c>: the field is
    ///     required to proceed.
    /// </summary>
    [JsonPropertyName("optional")]
    public bool? Optional { get; init; }
}
