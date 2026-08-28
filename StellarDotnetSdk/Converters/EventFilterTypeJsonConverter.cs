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
///     <c>filter type invalid: if set, type must be either 'system' or 'contract'</c>.
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
                    $"Value '{segment}' cannot be converted to type {nameof(EventFilterType)}. " +
                    "Stellar RPC accepts only 'system' and 'contract', comma-separated and without spaces."),
            };
        }

        return result;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="value" /> contains bits that are not defined <see cref="EventFilterType" />
    ///     flags — for example a raw cast such as <c>(EventFilterType)99</c>. Assigning such a value to
    ///     <see cref="GetEventsRequest.EventFilter.Type" /> already throws, so this is a backstop for values that
    ///     reach the serializer by another route.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, EventFilterType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToRequestValue());
    }
}
