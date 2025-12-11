using System;
using System.Text;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a single data entry attached to an account.
///     Account data entries are key-value pairs that can store arbitrary data on an account.
/// </summary>
public sealed class AccountDataResponse : Response
{
    /// <summary>
    ///     The base64-encoded value of the data entry.
    /// </summary>
    [JsonPropertyName("value")]
    public required string Value { get; init; }

    /// <summary>
    ///     The decoded UTF-8 string value of the data entry.
    /// </summary>
    public string ValueDecoded
    {
        get
        {
            var data = Convert.FromBase64String(Value);
            return Encoding.UTF8.GetString(data);
        }
    }
}