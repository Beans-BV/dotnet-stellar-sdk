using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Sep.Sep0012.Responses;

/// <summary>
///     Status of a customer's KYC process and the fields the anchor still needs or has received. Returned by
///     <c>GET /customer</c> and by the deprecated <c>PUT /customer/verification</c>, and posted by the anchor to the
///     URL registered with <c>PUT /customer/callback</c> (parse that payload with <see cref="FromJson" />).
/// </summary>
public sealed class GetCustomerInfoResponse
{
    // Throws on a lone surrogate instead of writing U+FFFD, which would turn malformed text into a valid payload.
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    /// <summary>
    ///     Gets the ID of the customer, if the customer has already been created via <c>PUT /customer</c>.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    ///     Gets the status of the customer's KYC process.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("status")]
    [JsonConverter(typeof(CustomerStatusJsonConverter))]
    public CustomerStatus Status { get; init; }

    /// <summary>
    ///     Gets the fields the anchor has not yet received for the customer and requested <c>type</c>, keyed by field
    ///     name. SEP-0012 requires it for <see cref="CustomerStatus.NeedsInfo" /> and allows it with any status. The
    ///     SDK does not enforce that requirement, so the status stays readable when an anchor omits the fields:
    ///     expect <c>null</c> here even for <see cref="CustomerStatus.NeedsInfo" />. A <c>null</c> entry fails the
    ///     parse on every deserialization path.
    /// </summary>
    [JsonPropertyName("fields")]
    [JsonConverter(typeof(CustomerFieldsJsonConverter))]
    public IReadOnlyDictionary<string, GetCustomerInfoField>? Fields { get; init; }

    /// <summary>
    ///     Gets the fields the anchor has received for the customer, keyed by field name, with their validation
    ///     status. Present whenever a provided field requires verification. A <c>null</c> entry fails the parse on
    ///     every deserialization path.
    /// </summary>
    [JsonPropertyName("provided_fields")]
    [JsonConverter(typeof(CustomerProvidedFieldsJsonConverter))]
    public IReadOnlyDictionary<string, GetCustomerInfoProvidedField>? ProvidedFields { get; init; }

    /// <summary>
    ///     Gets a human-readable message describing the current state of the KYC process. SEP-0012 requires it for
    ///     <see cref="CustomerStatus.Rejected" />. The SDK does not enforce that requirement, so a rejection without a
    ///     message still reaches the caller: expect <c>null</c> here even for <see cref="CustomerStatus.Rejected" />.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>
    ///     Parses a <c>GET /customer</c> response body, such as the payload an anchor POSTs to the URL registered with
    ///     <c>PUT /customer/callback</c>, using the SDK's hardened JSON settings: duplicate properties, a missing or
    ///     unknown <c>status</c>, unknown field types, a field without a <c>description</c>, and <c>null</c> entries
    ///     are all rejected.
    /// </summary>
    /// <remarks>
    ///     Verify the callback's <c>Signature</c> header with <see cref="KycCallbackSignature.Verify(string, byte[], string, Accounts.KeyPair, System.DateTimeOffset?, System.TimeSpan?)" /> before
    ///     trusting the payload.
    /// </remarks>
    /// <param name="json">The JSON body.</param>
    /// <returns>The parsed response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json" /> is <c>null</c>.</exception>
    /// <exception cref="JsonException">
    ///     Thrown when <paramref name="json" /> is not a valid SEP-0012 customer response. Its message is clamped and
    ///     free of control characters, since it can quote the payload's field names.
    /// </exception>
    public static GetCustomerInfoResponse FromJson(string json)
    {
        if (json == null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        byte[] utf8Json;
        try
        {
            utf8Json = StrictUtf8.GetBytes(json);
        }
        catch (EncoderFallbackException ex)
        {
            // A lone surrogate has no UTF-8 form; to a caller that is just another malformed payload.
            throw new JsonException("The customer response is not valid UTF-16 text.", ex);
        }

        return FromUtf8Json(utf8Json);
    }

    /// <summary>
    ///     Parses a customer response from its UTF-8 bytes. <see cref="FromJson" /> and <see cref="KycService" /> both
    ///     parse through this method, so a rule added here applies to the callback and the HTTP path alike.
    /// </summary>
    /// <param name="utf8Json">
    ///     The JSON body, already known to be valid UTF-8: System.Text.Json does not check the bytes of a value it
    ///     skips.
    /// </param>
    /// <exception cref="JsonException">Thrown as documented on <see cref="FromJson" />.</exception>
    internal static GetCustomerInfoResponse FromUtf8Json(ReadOnlySpan<byte> utf8Json)
    {
        try
        {
            return JsonSerializer.Deserialize<GetCustomerInfoResponse>(utf8Json, JsonOptions.DefaultOptions)
                   ?? throw new JsonException("The customer response is the JSON literal null.");
        }
        catch (JsonException ex)
        {
            // System.Text.Json's message ends with the JSON path, which quotes the payload's dictionary keys verbatim.
            throw UntrustedText.Sanitize(ex);
        }
    }
}
