using System.Text;

namespace StellarDotnetSdk.Sep.Sep0012.Requests;

/// <summary>
///     Request for <c>POST /customer/files</c>: upload a binary file on its own, so that <c>PUT /customer</c> can
///     reference it by <c>file_id</c> (see <see cref="PutCustomerInfoRequest.FileReferences" />) — useful when the
///     rest of the customer data is sent in a form that cannot carry files.
/// </summary>
public sealed record PostCustomerFileRequest
{
    /// <summary>
    ///     Gets the JWT obtained from the anchor through SEP-10 or SEP-45.
    /// </summary>
    public required string Jwt { get; init; }

    /// <summary>
    ///     Gets the file content, sent as the <c>file</c> part of a <c>multipart/form-data</c> body. Servers
    ///     typically reject files over about 10 MB with <c>413 Payload Too Large</c>.
    /// </summary>
    public required byte[] File { get; init; }

    /// <summary>
    ///     Gets the file name sent in the part's <c>Content-Disposition</c>. Defaults to <c>file</c>; many servers only
    ///     treat a part as an uploaded file when it has a file name.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    ///     Gets the part's <c>Content-Type</c>, for example <c>image/jpeg</c>. Defaults to
    ///     <c>application/octet-stream</c>.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    ///     Prints the request for <c>ToString</c> with the JWT and any customer data replaced by a placeholder.
    /// </summary>
    private bool PrintMembers(StringBuilder builder)
    {
        var start = builder.Length;
        RequestPrinter.Secret(builder, start, nameof(Jwt), Jwt);
        RequestPrinter.Value(builder, start, nameof(File), File == null ? null : $"[{File.Length} bytes]");
        RequestPrinter.Secret(builder, start, nameof(FileName), FileName);
        RequestPrinter.Value(builder, start, nameof(ContentType), ContentType);
        return true;
    }
}
