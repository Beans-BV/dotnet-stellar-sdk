using System;
using System.Collections.Generic;
using System.Text;

namespace StellarDotnetSdk.Sep.Sep0012.Requests;

/// <summary>
///     Request for the <b>deprecated</b> <c>PUT /customer/verification</c> endpoint. SEP-0012 v1.12.0 replaced it with
///     <see cref="PutCustomerInfoRequest.VerificationFields" /> on <c>PUT /customer</c>; use this only with anchors
///     that still require the old endpoint.
/// </summary>
[Obsolete("PUT /customer/verification is deprecated since SEP-0012 v1.12.0. Use PutCustomerInfoRequest.VerificationFields instead.")]
public sealed record PutCustomerVerificationRequest
{
    /// <summary>
    ///     Gets the JWT obtained from the anchor through SEP-10 or SEP-45.
    /// </summary>
    public required string Jwt { get; init; }

    /// <summary>
    ///     Gets the ID of the customer as returned by a previous <c>PUT /customer</c> request.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    ///     Gets the verification values, keyed by the SEP-0009 field being verified (for example
    ///     <c>mobile_number</c>). Each is sent as <c>&lt;field&gt;_verification</c>; a key that already ends in
    ///     <c>_verification</c> is sent unchanged, and a key that is only <c>_verification</c> is rejected. At least
    ///     one entry is required.
    /// </summary>
    public required IReadOnlyDictionary<string, string> VerificationFields { get; init; }

    /// <summary>
    ///     Prints the request for <c>ToString</c> with the JWT and any customer data replaced by a placeholder.
    /// </summary>
    private bool PrintMembers(StringBuilder builder)
    {
        var start = builder.Length;
        RequestPrinter.Secret(builder, start, nameof(Jwt), Jwt);
        RequestPrinter.Value(builder, start, nameof(Id), Id);
        RequestPrinter.Collection(builder, start, nameof(VerificationFields), VerificationFields?.Count);
        return true;
    }
}
