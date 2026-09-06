using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.SorobanRpc;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     Reads an array whose element type is non-nullable and rejects a <c>null</c> element with
///     <see cref="JsonException" />.
/// </summary>
/// <typeparam name="T">The element type, which the declaring property annotates as non-nullable.</typeparam>
/// <remarks>
///     <para>
///         <c>RespectNullableAnnotations</c> constrains the array <em>reference</em>, never its contents, so
///         <c>[null]</c> otherwise produced an array holding <see langword="null" /> despite a non-nullable
///         element type — and the compiler then suppressed exactly the null checks that would have caught it, so
///         a plain <c>foreach</c> over the result threw <see cref="NullReferenceException" /> at the caller
///         instead of failing at deserialization like every other malformed-payload path.
///     </para>
///     <para>
///         The check lives in a converter rather than in the property's <c>init</c> accessor because the two
///         entry points want different exception types, and a converter is the only place that can tell them
///         apart. System.Text.Json runs a property-level converter <em>before</em> assigning the value, so a
///         malformed payload fails here as a <see cref="JsonException" />; an accessor throwing
///         <see cref="ArgumentException" /> — the right type for a rejected argument — still guards the
///         ordinary C# path, and escapes <c>JsonSerializer.Deserialize</c> unwrapped, which is why it cannot
///         serve as the wire-format guard as well.
///     </para>
///     <para>
///         Each guarded property names a sealed subclass rather than closing this type directly, because a
///         <see cref="JsonConverterAttribute" /> can only name a type with an accessible parameterless
///         constructor. That is not merely a style constraint: the System.Text.Json source generator emits
///         <c>SYSLIB1220</c> and silently drops a converter it cannot construct, and the property's
///         <see cref="ArgumentException" /> guard would then surface out of
///         <c>JsonSerializer.Deserialize</c> in place of the <see cref="JsonException" /> documented here.
///     </para>
///     <para>
///         Attach these with a property-level <see cref="JsonConverterAttribute" />; never add one to
///         <see cref="JsonSerializerOptions.Converters" />. Both directions read and write the array by
///         delegating to <see cref="JsonSerializer" />, which is safe only while the converter is invisible to
///         the options being used — a global registration makes that delegation resolve back here and recurse
///         until the process dies. Because a <c>StackOverflowException</c> cannot be caught, and because these
///         types are necessarily public and look exactly like the converters this SDK does register globally,
///         both <see cref="Read" /> and <see cref="Write" /> check for that registration and raise an ordinary
///         <see cref="InvalidOperationException" /> instead.
///     </para>
///     <para>
///         This rejects nothing a conforming server can send: Stellar RPC's Go types are slices of structs (and,
///         for <c>events</c> and <c>auth</c>, of plain <c>string</c>) rather than of pointers, so
///         <c>encoding/json</c> cannot emit a null element for any of them. It is a guard against a
///         non-conforming or hostile endpoint.
///     </para>
/// </remarks>
public abstract class NonNullElementArrayJsonConverter<T> : JsonConverter<T[]?> where T : class
{
    private readonly string _wireName;

    /// <param name="wireName">The field's name as it appears in the JSON payload.</param>
    private protected NonNullElementArrayJsonConverter(string wireName)
    {
        _wireName = wireName;
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">Thrown when the array contains a <c>null</c> element.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when this converter has been registered on <see cref="JsonSerializerOptions.Converters" />
    ///     instead of being attached to a single property.
    /// </exception>
    public override T[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ThrowIfGloballyRegistered(options);

        // Deserializing the array itself cannot recurse into this converter: a property-level
        // [JsonConverter] binds to that property alone and is not consulted when resolving T[] from options.
        T[]? array;
        try
        {
            array = JsonSerializer.Deserialize<T[]>(ref reader, options);
        }
        catch (JsonException ex) when (ex.Path != null)
        {
            // Delegating the array read starts a fresh serializer session, which restarts the JSON path at
            // the array itself: EVERY failure inside a guarded array — a type mismatch, a [JsonRequired]
            // violation, anything the element's own converters raise — arrives with a path like "$[1].type"
            // that is relative to the array rather than to the payload.
            //
            // Rethrowing with Path left null is what repairs it: System.Text.Json fills an unset Path with
            // the ambient one it tracks in the OUTER session, which is the only thing that knows where this
            // array sits. Do not compute a path here instead. This converter knows its own field name but
            // not its ancestors, so "$." + _wireName is right only for a root-level property and silently
            // wrong for `auth`, which lives inside results[i] — it would report "$.results.auth[1]" and drop
            // the element index entirely. The relative locator survives inside the inner message.
            throw new JsonException($"Failed to read the '{_wireName}' array: {ex.Message}", ex);
        }

        if (array == null)
        {
            return null;
        }

        for (var i = 0; i < array.Length; i++)
        {
            if (array[i] is null)
            {
                // Path deliberately left unset, for the reason given in the catch above: System.Text.Json
                // supplies the ambient one, which is correct for a nested array too. The index goes in the
                // message rather than into a path this converter cannot compute.
                throw new JsonException($"The '{_wireName}' array contains a null element at index {i}.");
            }
        }

        return array;
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">
    ///     Thrown when the array contains a <c>null</c> element. Nothing of this array is written when it does,
    ///     but — as for any converter that fails part-way through a document — a <see cref="Utf8JsonWriter" />
    ///     the caller owns and reuses across serializations is left with unbalanced depth and should be
    ///     discarded rather than written to again. Serializing through
    ///     <see cref="JsonSerializer.Serialize(object?, Type, JsonSerializerOptions?)" /> and its overloads is
    ///     unaffected, because the writer does not outlive the call.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when this converter has been registered on <see cref="JsonSerializerOptions.Converters" />
    ///     instead of being attached to a single property.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, T[]? value, JsonSerializerOptions options)
    {
        ThrowIfGloballyRegistered(options);

        // Symmetric with Read. A property-level [JsonConverter] owns both directions, so nothing else —
        // RespectNullableAnnotations included — is left to police the element type on the way out. The
        // array is stored by reference and never copied, so a caller that still holds it can write a null
        // in after the accessor's check passed; without this, that null is serialized silently.
        if (value != null)
        {
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] is null)
                {
                    throw new JsonException(
                        $"The '{_wireName}' array contains a null element at index {i}.");
                }
            }
        }

        JsonSerializer.Serialize(writer, value, options);
    }

    /// <summary>
    ///     Rejects the one registration that turns this converter into infinite recursion.
    /// </summary>
    /// <remarks>
    ///     Both directions delegate to <see cref="JsonSerializer" /> for <c>T[]</c>. That is safe only while
    ///     this converter is invisible to <paramref name="options" /> — true of a property-level
    ///     <see cref="JsonConverterAttribute" />, false the moment an instance is added to
    ///     <see cref="JsonSerializerOptions.Converters" />, where the delegated call resolves straight back to
    ///     it. The resulting <c>StackOverflowException</c> terminates the process and cannot be caught, so it
    ///     is worth one dictionary lookup to convert it into an ordinary exception.
    ///     <para>
    ///         The check compares the resolved converter's exact runtime type, not the base type, so that the
    ///         message blames the right converter. Two sealed subclasses can close the same generic —
    ///         <c>events</c> and <c>results[i].auth</c> are both <c>string[]</c> — and misregistering either
    ///         one poisons every delegated <c>string[]</c> read, so the payload fails either way; that is
    ///         correct, because the options really are misconfigured. What differs is who reports it. Under a
    ///         base-type test the innocent converter attached to the property throws first and names the
    ///         shared base, pointing at neither the misregistered type nor the misconfigured option. Comparing
    ///         exact types lets it delegate once so the actually-misregistered converter resolves to itself
    ///         and reports its own name, which is the one the caller has to remove.
    ///     </para>
    /// </remarks>
    private void ThrowIfGloballyRegistered(JsonSerializerOptions options)
    {
        if (options.GetConverter(typeof(T[]))?.GetType() == GetType())
        {
            throw new InvalidOperationException(
                $"{GetType().Name} must be attached to a property with [JsonConverter], not registered on " +
                "JsonSerializerOptions.Converters: it reads and writes the array by delegating to " +
                "JsonSerializer, which would resolve back to this converter and recurse until the process " +
                "terminates.");
        }
    }
}

/// <summary>
///     Rejects a <c>null</c> element in <see cref="SimulateTransactionResponse.StateChanges" />.
/// </summary>
public sealed class StateChangesArrayJsonConverter
    : NonNullElementArrayJsonConverter<SimulateTransactionResponse.LedgerEntryChange>
{
    /// <summary>Creates the converter.</summary>
    public StateChangesArrayJsonConverter() : base("stateChanges")
    {
    }
}

/// <summary>
///     Rejects a <c>null</c> element in <see cref="SimulateTransactionResponse.Results" />.
/// </summary>
public sealed class SimulationResultsArrayJsonConverter
    : NonNullElementArrayJsonConverter<SimulateTransactionResponse.SimulateInvokeHostFunctionResult>
{
    /// <summary>Creates the converter.</summary>
    public SimulationResultsArrayJsonConverter() : base("results")
    {
    }
}

/// <summary>
///     Rejects a <c>null</c> element in <see cref="SimulateTransactionResponse.Events" />.
/// </summary>
public sealed class SimulationEventsArrayJsonConverter : NonNullElementArrayJsonConverter<string>
{
    /// <summary>Creates the converter.</summary>
    public SimulationEventsArrayJsonConverter() : base("events")
    {
    }
}

/// <summary>
///     Rejects a <c>null</c> element in
///     <see cref="SimulateTransactionResponse.SimulateInvokeHostFunctionResult.Auth" />.
/// </summary>
public sealed class SorobanAuthArrayJsonConverter : NonNullElementArrayJsonConverter<string>
{
    /// <summary>Creates the converter.</summary>
    public SorobanAuthArrayJsonConverter() : base("auth")
    {
    }
}
