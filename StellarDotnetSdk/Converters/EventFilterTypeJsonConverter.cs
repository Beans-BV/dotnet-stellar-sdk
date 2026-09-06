using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Requests.SorobanRpc;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for <see cref="EventFilterType" /> that maps between the flags enum and the single
///     comma-separated string Stellar RPC uses for an event filter's <c>type</c> field.
/// </summary>
/// <remarks>
///     The built-in <see cref="JsonStringEnumConverter" /> cannot be used here: for a <see cref="FlagsAttribute" />
///     enum it joins members with <c>", "</c> (a comma <em>and a space</em>), and RPC splits the value on a bare
///     comma without trimming, so <c>"system, contract"</c> is rejected with
///     <c>filter N invalid: filter type invalid: if set, type must be either 'system' or 'contract'</c>,
///     where <c>N</c> is the 1-based index of the offending filter.
/// </remarks>
public class EventFilterTypeJsonConverter : JsonConverter<EventFilterType>
{
    /// <inheritdoc />
    /// <exception cref="JsonException">
    ///     Thrown when the JSON value is not a string, or names an event type Stellar RPC does not accept.
    /// </exception>
    public override EventFilterType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(
                $"Expected a string value for {nameof(EventFilterType)} but found {reader.TokenType}.");
        }

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            // RPC treats an empty set as "no type filter", i.e. every event type.
            return EventFilterType.None;
        }

        var result = EventFilterType.None;
        foreach (var segment in value!.Split(','))
        {
            result |= segment switch
            {
                "system" => EventFilterType.System,
                "contract" => EventFilterType.Contract,
                _ => throw new JsonException(
                    $"Value {UntrustedJsonValue.Describe(segment)} cannot be converted to type " +
                    $"{nameof(EventFilterType)}. " +
                    "Stellar RPC accepts only 'system' and 'contract', comma-separated and without spaces."),
            };
        }

        return result;
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">
    ///     Thrown when <paramref name="value" /> contains bits that are not defined <see cref="EventFilterType" />
    ///     flags — for example a raw cast such as <c>(EventFilterType)99</c>. Assigning such a value to
    ///     <see cref="GetEventsRequest.EventFilter.Type" /> already throws
    ///     <see cref="ArgumentOutOfRangeException" />, so this is a backstop for values that reach the serializer
    ///     by another route.
    ///     <para>
    ///         The two exception types are deliberate and describe different failures: the setter rejects a bad
    ///         <em>argument</em>, while a converter reports a <em>serialization</em> failure and so throws what
    ///         the sibling converters throw — a caller wrapping <c>JsonSerializer.Serialize</c> in
    ///         <c>catch (JsonException)</c> catches all three.
    ///     </para>
    /// </exception>
    public override void Write(Utf8JsonWriter writer, EventFilterType value, JsonSerializerOptions options)
    {
        if (!value.IsDefined())
        {
            throw new JsonException($"Value '{value}' is not a defined {nameof(EventFilterType)}.");
        }

        writer.WriteStringValue(value.ToRequestValue());
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Required because this converter is attached to the enum <em>type</em>. Without the two property-name
    ///     overloads System.Text.Json has no way to turn the enum into a JSON object key and throws
    ///     <see cref="NotSupportedException" /> for a <c>Dictionary&lt;EventFilterType, T&gt;</c> — which is not a
    ///     <see cref="JsonException" />, so a caller's <c>catch (JsonException)</c> would miss it. Keys use the
    ///     same bare-comma wire spelling as values.
    /// </remarks>
    /// <exception cref="JsonException">Thrown when the key is not a valid filter-type spelling.</exception>
    public override EventFilterType ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return EventFilterType.None;
        }

        var result = EventFilterType.None;
        foreach (var segment in value!.Split(','))
        {
            result |= segment switch
            {
                "system" => EventFilterType.System,
                "contract" => EventFilterType.Contract,
                _ => throw new JsonException(
                    $"Value {UntrustedJsonValue.Describe(segment)} cannot be converted to type " +
                    $"{nameof(EventFilterType)}. " +
                    "Stellar RPC accepts only 'system' and 'contract', comma-separated and without spaces."),
            };
        }

        return result;
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">
    ///     Thrown when <paramref name="value" /> carries bits that are not defined
    ///     <see cref="EventFilterType" /> flags.
    /// </exception>
    public override void WriteAsPropertyName(Utf8JsonWriter writer, EventFilterType value,
        JsonSerializerOptions options)
    {
        if (!value.IsDefined())
        {
            throw new JsonException($"Value '{value}' is not a defined {nameof(EventFilterType)}.");
        }

        writer.WritePropertyName(value.ToRequestValue());
    }
}
