using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     Reads a JSON array into a read-only list whose element type is non-nullable, rejecting a <c>null</c>
///     element with <see cref="JsonException" />.
/// </summary>
/// <typeparam name="T">The element type, which the declaring property annotates as non-nullable.</typeparam>
/// <remarks>
///     <para>
///         The <see cref="IReadOnlyList{T}" /> counterpart of <see cref="NonNullElementArrayJsonConverter{T}" />, for
///         the same reason: <c>RespectNullableAnnotations</c> constrains the list reference, never its contents, so
///         <c>[null]</c> would otherwise yield a list holding <see langword="null" /> that the compiler then lets a
///         caller dereference without a check.
///     </para>
///     <para>
///         Attach it with a property-level <see cref="JsonConverterAttribute" /> naming the closed type (for example
///         <c>NonNullElementListJsonConverter&lt;DeliveryMethod&gt;</c>). It is public so that a consumer's
///         source-generated <see cref="JsonSerializerContext" /> can instantiate it.
///     </para>
///     <para>
///         Elements are read and written one at a time, delegating to <see cref="JsonSerializer" /> for <c>T</c>
///         alone. That is what a source-generated context supports: it carries metadata for the element type the
///         property exposes, but not for <c>T[]</c> or <see cref="IEnumerable{T}" />, which a whole-collection
///         delegation would need. It also means the converter needs no guard against a registration on
///         <see cref="JsonSerializerOptions.Converters" />, unlike <see cref="NonNullElementArrayJsonConverter{T}" />:
///         a delegated call for <c>T</c> never resolves back to a converter for <see cref="IReadOnlyList{T}" />.
///         Registered globally, it only applies the null-element check to every <see cref="IReadOnlyList{T}" /> of
///         that element type.
///     </para>
/// </remarks>
public sealed class NonNullElementListJsonConverter<T> : JsonConverter<IReadOnlyList<T>?> where T : class
{
    /// <inheritdoc />
    /// <exception cref="JsonException">
    ///     Thrown when the value is not an array, or the array contains a <c>null</c> or unreadable element.
    /// </exception>
    public override IReadOnlyList<T>? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected a JSON array, but found a {reader.TokenType} token.");
        }

        var items = new List<T>();
        while (true)
        {
            // The serializer hands a custom converter the whole value, so only a direct caller with a reader over a
            // partial buffer can run out here; that must fail rather than return the elements read so far.
            if (!reader.Read())
            {
                throw new JsonException($"The JSON ended before the array was closed, after {items.Count} elements.");
            }

            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.Null)
            {
                throw new JsonException($"The array contains a null element at index {items.Count}.");
            }

            T? item;
            try
            {
                item = JsonSerializer.Deserialize<T>(ref reader, options);
            }
            catch (JsonException ex) when (ex.Path != null)
            {
                // The delegated read starts a fresh serializer session whose path is relative to this element.
                // Leaving Path unset on the rethrow lets System.Text.Json fill in the outer, absolute one; the inner
                // message keeps the relative locator. See NonNullElementArrayJsonConverter for the full rationale.
                throw new JsonException($"Failed to read the array element at index {items.Count}: {ex.Message}",
                    ex);
            }

            // A JSON null is caught above; this is an element converter that answered null for a non-null token.
            items.Add(item ?? throw new JsonException(
                $"The array contains an element at index {items.Count} that was read as null."));
        }

        return items.AsReadOnly();
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">
    ///     Thrown when the list contains a <c>null</c> element; nothing of the list is written then.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, IReadOnlyList<T>? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        for (var i = 0; i < value.Count; i++)
        {
            if (value[i] is null)
            {
                throw new JsonException($"The list contains a null element at index {i}.");
            }
        }

        writer.WriteStartArray();
        foreach (var item in value)
        {
            JsonSerializer.Serialize(writer, item, options);
        }

        writer.WriteEndArray();
    }
}
