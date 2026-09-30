using System.Text;

namespace StellarDotnetSdk.Sep.Sep0012.Requests;

/// <summary>
///     Request for <c>DELETE /customer/[account]</c>: delete all personal information the anchor stores about a
///     customer. The request must be authenticated as the owner of <see cref="Account" />.
/// </summary>
public sealed record DeleteCustomerRequest
{
    /// <summary>
    ///     Gets the JWT obtained from the anchor through SEP-10 or SEP-45 for <see cref="Account" />.
    /// </summary>
    public required string Jwt { get; init; }

    /// <summary>
    ///     Gets the Stellar account (<c>G...</c>) or contract account (<c>C...</c>) of the customer to delete. It is
    ///     sent as a path segment.
    /// </summary>
    public required string Account { get; init; }

    /// <summary>
    ///     Gets the memo identifying the customer when <see cref="Account" /> is a shared account.
    /// </summary>
    public string? Memo { get; init; }

    /// <summary>
    ///     Gets the type of <see cref="Memo" />: <c>text</c>, <c>id</c>, or <c>hash</c>. <b>Deprecated</b> by SEP-0012.
    /// </summary>
    public string? MemoType { get; init; }

    /// <summary>
    ///     Prints the request for <c>ToString</c> with the JWT and any customer data replaced by a placeholder.
    /// </summary>
    private bool PrintMembers(StringBuilder builder)
    {
        var start = builder.Length;
        RequestPrinter.Secret(builder, start, nameof(Jwt), Jwt);
        RequestPrinter.Value(builder, start, nameof(Account), Account);
        RequestPrinter.Value(builder, start, nameof(Memo), Memo);
        RequestPrinter.Value(builder, start, nameof(MemoType), MemoType);
        return true;
    }
}
