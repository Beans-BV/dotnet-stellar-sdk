using System;
using System.Collections.Generic;
using System.Text.Json;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     Rejects JSON objects that define the same property name more than once.
/// </summary>
/// <remarks>
///     The SDK-wide duplicate-property guard on <see cref="JsonOptions" /> (<c>AllowDuplicateProperties = false</c>)
///     is enforced by the built-in object mapper only; converters that hand-parse JSON via
///     <see cref="JsonDocument" /> or a raw <see cref="Utf8JsonReader" /> loop are last-wins by default. Those
///     converters call into this class so a duplicated field (amount, asset code, time-lock predicate,
///     pagination href, ...) fails fast instead of being silently overridden by the last — potentially
///     attacker-appended — value. Duplicates are detected case-insensitively, matching the object mapper's
///     behavior under <c>PropertyNameCaseInsensitive = true</c>, and independently of the target framework's
///     System.Text.Json version.
/// </remarks>
internal static class JsonDuplicatePropertyGuard
{
    /// <summary>
    ///     Throws a <see cref="JsonException" /> if <paramref name="element" /> is a JSON object that defines
    ///     the same property name (case-insensitively) more than once. Elements other than objects are ignored.
    ///     Only the element's own properties are checked; nested objects are the responsibility of whichever
    ///     converter consumes them.
    /// </summary>
    /// <param name="element">The JSON element to check.</param>
    /// <param name="typeName">Name of the type being deserialized, used in the exception message.</param>
    internal static void EnsureNoDuplicateProperties(JsonElement element, string typeName)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var seen = CreateSeenSet();
        foreach (var property in element.EnumerateObject())
        {
            MarkSeen(seen, property.Name, typeName);
        }
    }

    /// <summary>
    ///     Creates the case-insensitive name set used with <see cref="MarkSeen" /> by converters that read
    ///     properties from a raw <see cref="Utf8JsonReader" /> loop.
    /// </summary>
    internal static HashSet<string> CreateSeenSet()
    {
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Records <paramref name="propertyName" /> in <paramref name="seen" /> and throws a
    ///     <see cref="JsonException" /> if it was already present (case-insensitively).
    /// </summary>
    /// <param name="seen">Set created by <see cref="CreateSeenSet" /> tracking the names read so far.</param>
    /// <param name="propertyName">The property name just read.</param>
    /// <param name="typeName">Name of the type being deserialized, used in the exception message.</param>
    internal static void MarkSeen(HashSet<string> seen, string propertyName, string typeName)
    {
        if (!seen.Add(propertyName))
        {
            throw new JsonException($"Duplicate property '{propertyName}' in {typeName} JSON.");
        }
    }
}
