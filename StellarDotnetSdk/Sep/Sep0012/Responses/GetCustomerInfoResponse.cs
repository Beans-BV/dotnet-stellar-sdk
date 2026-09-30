using System;
using System.Collections.Generic;
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
    ///     expect <c>null</c> here even for <see cref="CustomerStatus.NeedsInfo" />.
    /// </summary>
    [JsonPropertyName("fields")]
    public IReadOnlyDictionary<string, GetCustomerInfoField>? Fields { get; init; }

    /// <summary>
    ///     Gets the fields the anchor has received for the customer, keyed by field name, with their validation
    ///     status. Present whenever a provided field requires verification.
    /// </summary>
    [JsonPropertyName("provided_fields")]
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

        try
        {
            var response = JsonSerializer.Deserialize<GetCustomerInfoResponse>(json, JsonOptions.DefaultOptions)
                           ?? throw new JsonException("The customer response is the JSON literal null.");
            response.EnsureNoNullEntries();
            return response;
        }
        catch (JsonException ex)
        {
            // System.Text.Json's message ends with the JSON path, which quotes the payload's dictionary keys verbatim.
            throw KycUntrustedText.Sanitize(ex);
        }
        catch (ArgumentException ex)
        {
            // System.Text.Json transcodes the string to UTF-8 first and rejects a lone surrogate with an
            // ArgumentException; to a caller that is just another malformed payload.
            throw new JsonException("The customer response is not valid UTF-16 text.", ex);
        }
    }

    /// <summary>
    ///     Rejects a <c>null</c> value in <see cref="Fields" /> or <see cref="ProvidedFields" />.
    ///     <c>RespectNullableAnnotations</c> constrains the dictionary reference, never its values, so
    ///     <c>"fields": {"first_name": null}</c> would otherwise surface to callers as a
    ///     <see cref="System.NullReferenceException" /> through a non-nullable value type.
    /// </summary>
    internal void EnsureNoNullEntries()
    {
        if (Fields != null)
        {
            foreach (var entry in Fields)
            {
                if (entry.Value is null)
                {
                    throw new JsonException(
                        $"The 'fields' entry {UntrustedJsonValue.Describe(entry.Key)} is null.");
                }
            }
        }

        if (ProvidedFields != null)
        {
            foreach (var entry in ProvidedFields)
            {
                if (entry.Value is null)
                {
                    throw new JsonException(
                        $"The 'provided_fields' entry {UntrustedJsonValue.Describe(entry.Key)} is null.");
                }
            }
        }
    }
}
