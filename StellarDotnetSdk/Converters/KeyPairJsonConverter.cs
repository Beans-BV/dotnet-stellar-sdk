using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Accounts;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for <see cref="KeyPair" /> that serializes and deserializes Stellar account IDs
///     (public keys in StrKey format) as JSON string values.
/// </summary>
public class KeyPairJsonConverter : JsonConverter<KeyPair>
{
    public override KeyPair? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var accountId = reader.GetString();
        return accountId is null ? null : KeyPair.FromAccountId(accountId);
    }

    public override void Write(Utf8JsonWriter writer, KeyPair value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.AccountId);
    }
}