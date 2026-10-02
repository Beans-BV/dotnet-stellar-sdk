using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     Reads a JSON object into a string-keyed dictionary whose value type is non-nullable, and rejects a
///     <c>null</c> value with <see cref="JsonException" />.
/// </summary>
/// <typeparam name="TValue">The value type, which the declaring property annotates as non-nullable.</typeparam>
/// <remarks>
///     <para>
///         The dictionary counterpart of <see cref="NonNullElementArrayJsonConverter{T}" />; its remarks apply here
///         too. <c>RespectNullableAnnotations</c> constrains the dictionary reference, never its values, so
///         <c>{"first_name": null}</c> otherwise produced an entry holding <see langword="null" /> despite a
///         non-nullable value type, and the caller hit a <see cref="NullReferenceException" /> on first use.
///     </para>
///     <para>
///         The check lives in a converter, attached with a property-level <see cref="JsonConverterAttribute" />, so
///         that it runs on every deserialization path: the SDK's own parsing, a caller's options, and a caller's
///         source-generated <see cref="JsonSerializerContext" />. Each guarded property names a sealed subclass,
///         because the attribute can only name a type with an accessible parameterless constructor.
///     </para>
///     <para>
///         Never add one to <see cref="JsonSerializerOptions.Converters" />: both directions delegate to
///         <see cref="JsonSerializer" /> for the same dictionary type, which would then resolve back to this
///         converter and recurse until the process dies. Both directions check for that registration and raise an
///         <see cref="InvalidOperationException" /> instead. Writing delegates unchanged.
///     </para>
/// </remarks>
public abstract class NonNullValueDictionaryJsonConverter<TValue> : JsonConverter<IReadOnlyDictionary<string, TValue>?>
    where TValue : class
{
    private readonly string _wireName;

    /// <param name="wireName">The field's name as it appears in the JSON payload.</param>
    private protected NonNullValueDictionaryJsonConverter(string wireName)
    {
        _wireName = wireName;
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">Thrown when the object holds a <c>null</c> value.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when this converter has been registered on <see cref="JsonSerializerOptions.Converters" />
    ///     instead of being attached to a single property.
    /// </exception>
    public override IReadOnlyDictionary<string, TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        ThrowIfGloballyRegistered(options);

        IReadOnlyDictionary<string, TValue>? dictionary;
        try
        {
            dictionary = JsonSerializer.Deserialize<IReadOnlyDictionary<string, TValue>>(ref reader, options);
        }
        catch (JsonException ex) when (ex.Path != null)
        {
            // The delegated read restarts the JSON path at this object. Leaving Path unset lets System.Text.Json
            // fill in the outer one, as NonNullElementArrayJsonConverter explains.
            throw new JsonException($"Failed to read the '{_wireName}' object: {ex.Message}", ex);
        }

        // Unreachable through the serializer, which answers a JSON null itself for a reference type.
        if (dictionary == null)
        {
            return null;
        }

        foreach (var entry in dictionary)
        {
            if (entry.Value is null)
            {
                throw new JsonException(
                    $"The '{_wireName}' entry {UntrustedJsonValue.Describe(entry.Key)} is null.");
            }
        }

        return dictionary;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     Thrown when this converter has been registered on <see cref="JsonSerializerOptions.Converters" />
    ///     instead of being attached to a single property.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, IReadOnlyDictionary<string, TValue>? value,
        JsonSerializerOptions options)
    {
        ThrowIfGloballyRegistered(options);
        JsonSerializer.Serialize(writer, value, options);
    }

    /// <summary>
    ///     Rejects the one registration that turns this converter into infinite recursion.
    /// </summary>
    private void ThrowIfGloballyRegistered(JsonSerializerOptions options)
    {
        if (options.GetConverter(typeof(IReadOnlyDictionary<string, TValue>))?.GetType() == GetType())
        {
            throw new InvalidOperationException(
                $"{GetType().Name} must be attached to a property with [JsonConverter], not registered on " +
                "JsonSerializerOptions.Converters: it reads and writes the dictionary by delegating to " +
                "JsonSerializer, which would resolve back to this converter and recurse until the process " +
                "terminates.");
        }
    }
}
