using System.Text;

namespace StellarDotnetSdk.Sep.Sep0012.Requests;

/// <summary>
///     Request for <c>GET /customer</c>: fetch the fields the anchor needs to register a customer, or check the KYC
///     status of a customer that may already be registered.
/// </summary>
/// <remarks>
///     With no identifying parameter the anchor identifies the customer from the <c>sub</c> of the JWT (or returns the
///     fields required for a new customer). Once <c>PUT /customer</c> has returned an <see cref="Id" />, prefer it
///     over <see cref="Account" /> and <see cref="Memo" />.
/// </remarks>
public sealed record GetCustomerInfoRequest
{
    /// <summary>
    ///     Gets the JWT obtained from the anchor through SEP-10 (<see cref="StellarDotnetSdk.Sep.Sep0010.ClientWebAuth" />)
    ///     or, for contract accounts, SEP-45 (<see cref="StellarDotnetSdk.Sep.Sep0045.ClientWebAuthContract" />).
    ///     Sent as <c>Authorization: Bearer &lt;JWT&gt;</c>.
    /// </summary>
    public required string Jwt { get; init; }

    /// <summary>
    ///     Gets the ID of the customer as returned by a previous <c>PUT /customer</c> request.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    ///     Gets the customer's account (<c>G...</c>, <c>M...</c> or <c>C...</c>). <b>Deprecated</b> by SEP-0012: the
    ///     server infers the account from the JWT's <c>sub</c>; if sent, it must match that value.
    /// </summary>
    public string? Account { get; init; }

    /// <summary>
    ///     Gets the client-generated memo that uniquely identifies a customer of a shared (omnibus) account. Must match
    ///     the memo in the JWT's <c>sub</c> if there is one, and must not be set for a <c>C...</c> account.
    /// </summary>
    public string? Memo { get; init; }

    /// <summary>
    ///     Gets the type of <see cref="Memo" />: <c>text</c>, <c>id</c>, or <c>hash</c>. <b>Deprecated</b> by SEP-0012,
    ///     since memos should always be of type <c>id</c>.
    /// </summary>
    public string? MemoType { get; init; }

    /// <summary>
    ///     Gets the type of action the customer is being KYC'd for (for example <c>sep6</c>, <c>sep31-sender</c>,
    ///     <c>sep31-receiver</c>). Recommended on every request, since a customer can have a different status per type;
    ///     required when <see cref="TransactionId" /> is set.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    ///     Gets the transaction the customer's information is associated with, for anchors whose requirements depend on
    ///     the transaction (for example, more information for larger amounts).
    /// </summary>
    public string? TransactionId { get; init; }

    /// <summary>
    ///     Gets the ISO 639-1 language code for human-readable descriptions, choices and messages. Defaults to
    ///     <c>en</c> on the server.
    /// </summary>
    public string? Lang { get; init; }

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
        RequestPrinter.Value(builder, start, nameof(Lang), Lang);
        return true;
    }
}
