using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for polymorphic Predicate deserialization.
///     Determines the concrete predicate type by inspecting which property is present in the JSON.
/// </summary>
/// <remarks>
///     <para>
///         The Horizon API returns predicates with different properties for different types:
///     </para>
///     <list type="bullet">
///         <item><c>{"and": [...]}</c> → <see cref="PredicateAnd" /></item>
///         <item><c>{"or": [...]}</c> → <see cref="PredicateOr" /></item>
///         <item><c>{"not": {...}}</c> → <see cref="PredicateNot" /></item>
///         <item><c>{"unconditional": true}</c> → <see cref="PredicateUnconditional" /></item>
///         <item><c>{"abs_before": "...", "abs_before_epoch": ...}</c> → <see cref="PredicateBeforeAbsoluteTime" /></item>
///         <item><c>{"rel_before": ...}</c> → <see cref="PredicateBeforeRelativeTime" /></item>
///     </list>
/// </remarks>
public class PredicateJsonConverter : JsonConverter<Predicate>
{
    public override Predicate? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        // Determine type by which property is present
        if (root.TryGetProperty("and", out var andElement))
        {
            var predicates = DeserializePredicateArray(andElement, options);
            if (predicates.Length < 2)
            {
                throw new JsonException(
                    "Property 'and' must contain at least 2 predicates.");
            }

            return new PredicateAnd(predicates[0], predicates[1]);
        }

        if (root.TryGetProperty("or", out var orElement))
        {
            var predicates = DeserializePredicateArray(orElement, options);
            if (predicates.Length < 2)
            {
                throw new JsonException(
                    "Property 'or' must contain at least 2 predicates.");
            }

            return new PredicateOr(predicates[0], predicates[1]);
        }

        if (root.TryGetProperty("not", out var notElement))
        {
            var inner = JsonSerializer.Deserialize<Predicate>(notElement.GetRawText(), options);
            if (inner == null)
            {
                throw new JsonException(
                    "Property 'not' must contain a valid predicate.");
            }

            return new PredicateNot(inner);
        }

        if (root.TryGetProperty("unconditional", out var unconditionalElement) &&
            unconditionalElement.ValueKind == JsonValueKind.True)
        {
            return new PredicateUnconditional();
        }

        if (root.TryGetProperty("abs_before", out var absBeforeElement))
        {
            var absBefore = absBeforeElement.GetString();
            if (string.IsNullOrEmpty(absBefore))
            {
                throw new JsonException(
                    "Property 'abs_before' must be a non-empty string.");
            }

            long? absBeforeEpoch = null;
            if (root.TryGetProperty("abs_before_epoch", out var epochElement))
            {
                absBeforeEpoch = epochElement.ValueKind == JsonValueKind.String
                    ? long.Parse(epochElement.GetString()!)
                    : epochElement.GetInt64();
            }

            return new PredicateBeforeAbsoluteTime(absBefore, absBeforeEpoch);
        }

        if (root.TryGetProperty("rel_before", out var relBeforeElement))
        {
            var relBefore = relBeforeElement.ValueKind == JsonValueKind.String
                ? long.Parse(relBeforeElement.GetString()!)
                : relBeforeElement.GetInt64();

            return new PredicateBeforeRelativeTime(relBefore);
        }

        throw new JsonException(
            "Invalid Predicate: no recognized predicate type found. " +
            "Expected one of: 'and', 'or', 'not', 'unconditional', 'abs_before', or 'rel_before'.");
    }

    public override void Write(Utf8JsonWriter writer, Predicate value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        switch (value)
        {
            case PredicateAnd and:
                writer.WritePropertyName("and");
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, and.Left, options);
                JsonSerializer.Serialize(writer, and.Right, options);
                writer.WriteEndArray();
                break;

            case PredicateOr or:
                writer.WritePropertyName("or");
                writer.WriteStartArray();
                JsonSerializer.Serialize(writer, or.Left, options);
                JsonSerializer.Serialize(writer, or.Right, options);
                writer.WriteEndArray();
                break;

            case PredicateNot not:
                writer.WritePropertyName("not");
                JsonSerializer.Serialize(writer, not.Inner, options);
                break;

            case PredicateUnconditional:
                writer.WriteBoolean("unconditional", true);
                break;

            case PredicateBeforeAbsoluteTime abs:
                writer.WriteString("abs_before", abs.AbsBefore);
                if (abs.AbsBeforeEpoch.HasValue)
                {
                    writer.WriteNumber("abs_before_epoch", abs.AbsBeforeEpoch.Value);
                }

                break;

            case PredicateBeforeRelativeTime rel:
                writer.WriteNumber("rel_before", rel.RelBefore);
                break;

            default:
                throw new JsonException($"Unknown predicate type: {value.GetType().Name}");
        }

        writer.WriteEndObject();
    }

    private static Predicate[] DeserializePredicateArray(JsonElement element, JsonSerializerOptions options)
    {
        var length = element.GetArrayLength();
        var result = new Predicate[length];
        var index = 0;
        foreach (var item in element.EnumerateArray())
        {
            var predicate = JsonSerializer.Deserialize<Predicate>(item.GetRawText(), options);
            if (predicate == null)
            {
                throw new JsonException($"Failed to deserialize predicate at index {index}.");
            }

            result[index++] = predicate;
        }

        return result;
    }
}