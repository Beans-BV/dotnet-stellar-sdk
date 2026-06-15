using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0045;

/// <summary>Response from a SEP-45 WebAuth server GET /auth endpoint.</summary>
[JsonConverter(typeof(ChallengeForContractsResponseConverter))]
public sealed record ChallengeForContractsResponse
{
    /// <summary>Base64-encoded XDR of the SorobanAuthorizationEntry list to sign.</summary>
    public string? AuthorizationEntries { get; init; }

    /// <summary>Network passphrase the challenge is bound to.</summary>
    public string? NetworkPassphrase { get; init; }
}

/// <summary>
///     Deserializes the SEP-45 challenge response, accepting BOTH snake_case
///     (<c>authorization_entries</c>, <c>network_passphrase</c>) and camelCase
///     (<c>authorizationEntries</c>, <c>networkPassphrase</c>) field names. snake_case is the SEP-45
///     canonical form and takes precedence; the camelCase fallback matches the Flutter peer SDK so a
///     camelCase-emitting server interoperates. Field names are matched exactly (not case-insensitively),
///     mirroring the peer SDK's two-key lookup.
/// </summary>
/// <remarks>
///     This response is an adversarial-server surface (its <c>authorization_entries</c> blob is what the
///     client signs), so the converter preserves the SDK-wide duplicate-property guard
///     (<see cref="StellarDotnetSdk.Converters.JsonOptions" /> sets <c>AllowDuplicateProperties = false</c>):
///     a raw <see cref="JsonDocument" /> is last-wins by default, so duplicates are rejected explicitly
///     here rather than silently overriding the signed blob.
/// </remarks>
internal sealed class ChallengeForContractsResponseConverter : JsonConverter<ChallengeForContractsResponse>
{
    public override ChallengeForContractsResponse Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            return new ChallengeForContractsResponse();
        }

        string? snakeAuthorizationEntries = null, camelAuthorizationEntries = null;
        string? snakeNetworkPassphrase = null, camelNetworkPassphrase = null;
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var property in root.EnumerateObject())
        {
            // Reject duplicate property names — a raw JsonDocument is last-wins, which would bypass the
            // AllowDuplicateProperties = false guard the SDK applies to every other response.
            if (!seen.Add(property.Name))
            {
                throw new JsonException(
                    $"Duplicate property '{property.Name}' in SEP-45 challenge response.");
            }

            if (property.Value.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            switch (property.Name)
            {
                case "authorization_entries": snakeAuthorizationEntries = property.Value.GetString(); break;
                case "authorizationEntries": camelAuthorizationEntries = property.Value.GetString(); break;
                case "network_passphrase": snakeNetworkPassphrase = property.Value.GetString(); break;
                case "networkPassphrase": camelNetworkPassphrase = property.Value.GetString(); break;
            }
        }

        return new ChallengeForContractsResponse
        {
            AuthorizationEntries = snakeAuthorizationEntries ?? camelAuthorizationEntries,
            NetworkPassphrase = snakeNetworkPassphrase ?? camelNetworkPassphrase,
        };
    }

    public override void Write(
        Utf8JsonWriter writer, ChallengeForContractsResponse value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value.AuthorizationEntries != null)
        {
            writer.WriteString("authorization_entries", value.AuthorizationEntries);
        }
        if (value.NetworkPassphrase != null)
        {
            writer.WriteString("network_passphrase", value.NetworkPassphrase);
        }
        writer.WriteEndObject();
    }
}