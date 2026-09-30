using System.Text;

namespace StellarDotnetSdk.Sep.Sep0012.Requests;

/// <summary>
///     Builds the <c>ToString</c> text of the SEP-0012 request records. The compiler-generated form prints every
///     property, which would put the bearer JWT and the customer's KYC data into any log line that formats a request;
///     these helpers print identifiers (customer ID, account, memo, transaction ID) as they are, reduce a URL to its
///     origin, and replace secrets and personal data with a placeholder.
/// </summary>
/// <remarks>
///     A record's <c>ToString</c> writes <c>TypeName { </c> before calling <c>PrintMembers</c>, so each helper takes
///     the builder length at which the members start, to know whether a separator is needed.
/// </remarks>
internal static class RequestPrinter
{
    private const string Redacted = "[redacted]";

    internal static void Value(StringBuilder builder, int start, string name, object? value)
    {
        Name(builder, start, name).Append(value);
    }

    internal static void Secret(StringBuilder builder, int start, string name, object? value)
    {
        Name(builder, start, name).Append(value == null ? null : Redacted);
    }

    /// <summary>
    ///     Prints only a URL's scheme, host and port. Every other part can carry a secret — a webhook URL often holds a
    ///     per-customer token in its path or query — so anything beyond the origin is replaced by a placeholder.
    /// </summary>
    internal static void Url(StringBuilder builder, int start, string name, string? value)
    {
        var target = Name(builder, start, name);
        if (value == null)
        {
            return;
        }

        if (!System.Uri.TryCreate(value.Trim(), System.UriKind.Absolute, out var uri))
        {
            target.Append(Redacted);
            return;
        }

        target.Append(uri.GetComponents(System.UriComponents.SchemeAndServer, System.UriFormat.UriEscaped));
        if (uri.UserInfo.Length > 0 || uri.AbsolutePath != "/" || uri.Query.Length > 0 || uri.Fragment.Length > 0)
        {
            target.Append(' ').Append(Redacted);
        }
    }

    internal static void Collection(StringBuilder builder, int start, string name, int? count)
    {
        var target = Name(builder, start, name);
        if (count != null)
        {
            target.Append('[').Append(count.Value).Append(count.Value == 1 ? " entry, " : " entries, ")
                .Append(Redacted).Append(']');
        }
    }

    private static StringBuilder Name(StringBuilder builder, int start, string name)
    {
        if (builder.Length > start)
        {
            builder.Append(", ");
        }

        return builder.Append(name).Append(" = ");
    }
}
