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
///         <c>NonNullElementListJsonConverter&lt;DeliveryMethod&gt;</c>); never register it on
///         <see cref="JsonSerializerOptions.Converters" />. Reading delegates to <see cref="JsonSerializer" /> for
///         <c>T[]</c> and writing for <see cref="IEnumerable{T}" />, neither of which resolves back to this
///         converter while it is property-scoped.
///     </para>
///     <para>
///         Kept <c>internal</c>, so it adds nothing to the public surface.
///     </para>
/// </remarks>
internal sealed class NonNullElementListJsonConverter<T> : JsonConverter<IReadOnlyList<T>?> where T : class
{
    /// <inheritdoc />
    /// <exception cref="JsonException">Thrown when the array contains a <c>null</c> element.</exception>
    public override IReadOnlyList<T>? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        T[]? array;
        try
        {
            array = JsonSerializer.Deserialize<T[]>(ref reader, options);
        }
        catch (JsonException ex) when (ex.Path != null)
        {
            // The delegated read starts a fresh serializer session whose path is relative to this array. Leaving
            // Path unset on the rethrow lets System.Text.Json fill in the outer, absolute one; the inner message
            // keeps the relative locator. See NonNullElementArrayJsonConverter for the full rationale.
            throw new JsonException($"Failed to read the array: {ex.Message}", ex);
        }

        // Unreachable through the serializer, which answers a JSON null itself for a reference type.
        if (array == null)
        {
            return null;
        }

        for (var i = 0; i < array.Length; i++)
        {
            if (array[i] is null)
            {
                throw new JsonException($"The array contains a null element at index {i}.");
            }
        }

        return Array.AsReadOnly(array);
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">Thrown when the list contains a <c>null</c> element.</exception>
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

        JsonSerializer.Serialize<IEnumerable<T>>(writer, value, options);
    }
}
