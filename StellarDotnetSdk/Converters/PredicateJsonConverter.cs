using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Predicates;

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
///     <para>
///         Duplicate JSON property names are always rejected with a <see cref="JsonException" />, matched
///         case-insensitively, at every nesting level. This is intentional hardening for time-lock
///         predicates and does not honor the <see cref="JsonSerializerOptions" /> passed to
///         <see cref="Read" /> — neither <see cref="JsonSerializerOptions.AllowDuplicateProperties" />
///         nor <see cref="JsonSerializerOptions.PropertyNameCaseInsensitive" /> changes it.
///     </para>
/// </remarks>
public class PredicateJsonConverter : JsonConverter<Predicate>
{
    /// <inheritdoc />
    public override Predicate? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        // Nested predicates re-enter this converter, so each recursion level checks its own object.
        JsonDuplicatePropertyGuard.EnsureNoDuplicateProperties(root, nameof(Predicate));

        // Determine type by which property is present
        if (root.TryGetProperty("and", out var andElement))
        {
            var predicates = DeserializePredicateArray(andElement, options, "and");
            return new PredicateAnd(predicates[0], predicates[1]);
        }

        if (root.TryGetProperty("or", out var orElement))
        {
            var predicates = DeserializePredicateArray(orElement, options, "or");
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
                absBeforeEpoch = ReadInt64FromNumberOrString(epochElement, "abs_before_epoch");

                // Horizon renders abs_before and abs_before_epoch as the same instant. If a payload
                // supplies both and they disagree, a spoofed epoch could silently shift the claim
                // deadline while the human-readable string still looks correct (the DateTime accessor
                // prefers the epoch), so reject the contradiction instead of trusting one side.
                if (DateTimeOffset.TryParse(absBefore, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var parsedAbsBefore)
                    && parsedAbsBefore.ToUnixTimeSeconds() != absBeforeEpoch.Value)
                {
                    throw new JsonException(
                        $"Property 'abs_before_epoch' ({absBeforeEpoch.Value}) does not match " +
                        $"'abs_before' ({absBefore}); they must denote the same instant.");
                }
            }

            return new PredicateBeforeAbsoluteTime(absBefore, absBeforeEpoch);
        }

        if (root.TryGetProperty("rel_before", out var relBeforeElement))
        {
            var relBefore = ReadInt64FromNumberOrString(relBeforeElement, "rel_before");

            return new PredicateBeforeRelativeTime(relBefore);
        }

        throw new JsonException(
            "Invalid Predicate: no recognized predicate type found. " +
            "Expected one of: 'and', 'or', 'not', 'unconditional', 'abs_before', or 'rel_before'.");
    }

    /// <inheritdoc />
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

    /// <summary>
    ///     Reads a non-negative 64-bit integer that Horizon emits either as a JSON number or as a numeric
    ///     string. Every malformed value becomes a <see cref="JsonException" /> (the SDK's documented
    ///     failure mode) rather than a leaked <see cref="FormatException" /> or
    ///     <see cref="OverflowException" />. Stellar time bounds are unsigned, so a negative value is
    ///     rejected as well.
    /// </summary>
    private static long ReadInt64FromNumberOrString(JsonElement element, string propertyName)
    {
        long value;
        switch (element.ValueKind)
        {
            case JsonValueKind.String
                when long.TryParse(element.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out var parsed):
                value = parsed;
                break;
            case JsonValueKind.Number when element.TryGetInt64(out var number):
                value = number;
                break;
            default:
                throw new JsonException(
                    $"Property '{propertyName}' must be a 64-bit integer or a numeric string containing one.");
        }

        if (value < 0)
        {
            throw new JsonException(
                $"Property '{propertyName}' must not be negative; Stellar time bounds are unsigned.");
        }

        return value;
    }

    private static Predicate[] DeserializePredicateArray(JsonElement element, JsonSerializerOptions options,
        string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException($"Property '{propertyName}' must be an array of exactly 2 predicates.");
        }

        // Validate the arity before touching any element: Stellar's ClaimPredicate AND/OR are strictly
        // binary (extra elements used to be dropped silently), and checking first also stops an
        // oversized array from being fully materialized.
        var length = element.GetArrayLength();
        if (length != 2)
        {
            throw new JsonException(
                $"Property '{propertyName}' must contain exactly 2 predicates, but found {length}.");
        }

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