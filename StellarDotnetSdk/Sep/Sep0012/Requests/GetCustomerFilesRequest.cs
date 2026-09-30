using System.Text;

namespace StellarDotnetSdk.Sep.Sep0012.Requests;

/// <summary>
///     Request for <c>GET /customer/files</c>: fetch metadata of files uploaded with <c>POST /customer/files</c>.
///     SEP-0012 requires one of <see cref="FileId" /> or <see cref="CustomerId" />.
/// </summary>
public sealed record GetCustomerFilesRequest
{
    /// <summary>
    ///     Gets the JWT obtained from the anchor through SEP-10 or SEP-45.
    /// </summary>
    public required string Jwt { get; init; }

    /// <summary>
    ///     Gets the <c>file_id</c> returned by a previous <c>POST /customer/files</c> request. The response then lists
    ///     that single file.
    /// </summary>
    public string? FileId { get; init; }

    /// <summary>
    ///     Gets the customer <c>id</c> returned by a previous <c>PUT /customer</c> request. The response then lists every
    ///     file uploaded for that customer.
    /// </summary>
    public string? CustomerId { get; init; }

    /// <summary>
    ///     Prints the request for <c>ToString</c> with the JWT and any customer data replaced by a placeholder.
    /// </summary>
    private bool PrintMembers(StringBuilder builder)
    {
        var start = builder.Length;
        RequestPrinter.Secret(builder, start, nameof(Jwt), Jwt);
        RequestPrinter.Value(builder, start, nameof(FileId), FileId);
        RequestPrinter.Value(builder, start, nameof(CustomerId), CustomerId);
        return true;
    }
}
