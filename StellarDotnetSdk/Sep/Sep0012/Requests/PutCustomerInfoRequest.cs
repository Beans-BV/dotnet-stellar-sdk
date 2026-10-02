using System.Collections.Generic;
using StellarDotnetSdk.Sep.Sep0009;
using System.Text;

namespace StellarDotnetSdk.Sep.Sep0012.Requests;

/// <summary>
///     Request for <c>PUT /customer</c>: idempotently upload customer information to the anchor. Sent as
///     <c>multipart/form-data</c>, with every text field before any binary field as SEP-0012 requires.
/// </summary>
/// <remarks>
///     <para>
///         Fields are written in this order, a later source overriding an earlier one with the same name:
///         identification parameters, <see cref="KycFields" />, <see cref="CustomFields" />,
///         <see cref="VerificationFields" />, <see cref="FileReferences" />; then the binary parts from
///         <see cref="KycFields" /> followed by <see cref="CustomFiles" />. A name used by both a text field and a
///         binary field (for example <c>CustomFields["photo_id_front"]</c> with
///         <see cref="NaturalPersonKycFields.PhotoIdFront" />) is rejected with an
///         <see cref="System.ArgumentException" /> before sending.
///     </para>
/// </remarks>
public sealed record PutCustomerInfoRequest
{
    /// <summary>
    ///     Gets the JWT obtained from the anchor through SEP-10 (<see cref="StellarDotnetSdk.Sep.Sep0010.ClientWebAuth" />)
    ///     or SEP-45 (<see cref="StellarDotnetSdk.Sep.Sep0045.ClientWebAuthContract" />).
    /// </summary>
    public required string Jwt { get; init; }

    /// <summary>
    ///     Gets the <c>id</c> returned by a previous <c>PUT /customer</c> request. If set, no other identification
    ///     parameter is required.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    ///     Gets the customer's account (<c>G...</c>, <c>M...</c> or <c>C...</c>). <b>Deprecated</b> by SEP-0012: the
    ///     server infers the account from the JWT's <c>sub</c>; if sent, it must match that value.
    /// </summary>
    public string? Account { get; init; }

    /// <summary>
    ///     Gets the client-generated memo that uniquely identifies a customer of a shared (omnibus) account.
    /// </summary>
    public string? Memo { get; init; }

    /// <summary>
    ///     Gets the type of <see cref="Memo" />: <c>text</c>, <c>id</c>, or <c>hash</c>. <b>Deprecated</b> by SEP-0012.
    /// </summary>
    public string? MemoType { get; init; }

    /// <summary>
    ///     Gets the type of the customer as defined by the SEP using SEP-0012 (for example <c>sep31-sender</c>).
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    ///     Gets the transaction the customer's information is associated with.
    /// </summary>
    public string? TransactionId { get; init; }

    /// <summary>
    ///     Gets the SEP-0009 fields to submit: natural-person and/or organization fields, including their nested
    ///     financial-account and card fields. Binary SEP-0009 fields (photos, proofs) are sent as file parts.
    /// </summary>
    public StandardKycFields? KycFields { get; init; }

    /// <summary>
    ///     Gets additional non-SEP-0009 text fields the anchor asked for, keyed by field name.
    /// </summary>
    public IReadOnlyDictionary<string, string>? CustomFields { get; init; }

    /// <summary>
    ///     Gets additional binary fields the anchor asked for, keyed by field name. Sent after all text fields.
    /// </summary>
    public IReadOnlyDictionary<string, byte[]>? CustomFiles { get; init; }

    /// <summary>
    ///     Gets verification values (for example a confirmation code) for fields in the
    ///     <c>VERIFICATION_REQUIRED</c> status, keyed by the SEP-0009 field name (for example <c>mobile_number</c>).
    ///     Each is sent as <c>&lt;field&gt;_verification</c>, the SEP-0012 v1.12.0 replacement for the deprecated
    ///     <c>PUT /customer/verification</c>; a key that already ends in <c>_verification</c> is sent unchanged, and a
    ///     key that is only <c>_verification</c> is rejected.
    /// </summary>
    public IReadOnlyDictionary<string, string>? VerificationFields { get; init; }

    /// <summary>
    ///     Gets references to files uploaded with <c>POST /customer/files</c>, keyed by the SEP-0009 field name (for
    ///     example <c>photo_id_front</c>) with the returned <c>file_id</c> as value. Each is sent as
    ///     <c>&lt;field&gt;_file_id</c>; a key that already ends in <c>_file_id</c> is sent unchanged, and a key that is
    ///     only <c>_file_id</c> is rejected.
    /// </summary>
    public IReadOnlyDictionary<string, string>? FileReferences { get; init; }

    /// <summary>
    ///     Prints the request for <c>ToString</c> with the JWT and any customer data replaced by a placeholder.
    /// </summary>
    private bool PrintMembers(StringBuilder builder)
    {
        var start = builder.Length;
        RequestPrinter.Secret(builder, start, nameof(Jwt), Jwt);
        RequestPrinter.Value(builder, start, nameof(Id), Id);
        RequestPrinter.Value(builder, start, nameof(Account), Account);
        RequestPrinter.Value(builder, start, nameof(Memo), Memo);
        RequestPrinter.Value(builder, start, nameof(MemoType), MemoType);
        RequestPrinter.Value(builder, start, nameof(Type), Type);
        RequestPrinter.Value(builder, start, nameof(TransactionId), TransactionId);
        RequestPrinter.Secret(builder, start, nameof(KycFields), KycFields);
        RequestPrinter.Collection(builder, start, nameof(CustomFields), CustomFields?.Count);
        RequestPrinter.Collection(builder, start, nameof(CustomFiles), CustomFiles?.Count);
        RequestPrinter.Collection(builder, start, nameof(VerificationFields), VerificationFields?.Count);
        RequestPrinter.Collection(builder, start, nameof(FileReferences), FileReferences?.Count);
        return true;
    }
}
