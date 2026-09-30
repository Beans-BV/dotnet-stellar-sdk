using System.Text;

namespace StellarDotnetSdk.Sep.Sep0012.Requests;

/// <summary>
///     Request for <c>PUT /customer/callback</c>: register a URL the anchor POSTs to whenever the customer's
///     <c>status</c> changes, replacing any previously registered URL. The anchor stops once the status becomes
///     <c>ACCEPTED</c> or <c>REJECTED</c>.
/// </summary>
/// <remarks>
///     Each callback carries a <c>GET /customer</c> response body (parse it with
///     <see cref="Responses.GetCustomerInfoResponse.FromJson" />) and a <c>Signature</c> header to check with
///     <see cref="KycCallbackSignature.Verify(string, byte[], string, Accounts.KeyPair, System.DateTimeOffset?, System.TimeSpan?)" /> against the anchor's stellar.toml <c>SIGNING_KEY</c>.
/// </remarks>
public sealed record PutCustomerCallbackRequest
{
    /// <summary>
    ///     Gets the JWT obtained from the anchor through SEP-10 or SEP-45.
    /// </summary>
    public required string Jwt { get; init; }

    /// <summary>
    ///     Gets the callback URL the anchor POSTs status changes to.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    ///     Gets the ID of the customer as returned by a previous <c>PUT /customer</c> request.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    ///     Gets the customer's account. <b>Deprecated</b> by SEP-0012: the server infers it from the JWT's <c>sub</c>.
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
    ///     Prints the request for <c>ToString</c> with the JWT redacted and the callback URL reduced to its origin.
    /// </summary>
    private bool PrintMembers(StringBuilder builder)
    {
        var start = builder.Length;
        RequestPrinter.Secret(builder, start, nameof(Jwt), Jwt);
        RequestPrinter.Url(builder, start, nameof(Url), Url);
        RequestPrinter.Value(builder, start, nameof(Id), Id);
        RequestPrinter.Value(builder, start, nameof(Account), Account);
        RequestPrinter.Value(builder, start, nameof(Memo), Memo);
        RequestPrinter.Value(builder, start, nameof(MemoType), MemoType);
        return true;
    }
}
