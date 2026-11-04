using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a paginated page of objects from the Horizon API.
///     Used to navigate through large result sets using cursor-based pagination.
/// </summary>
/// <typeparam name="T">The type of records contained in this page.</typeparam>
public sealed class Page<T> : Response
{
    /// <summary>
    ///     The embedded records container for HAL JSON format.
    /// </summary>
    [JsonPropertyName("_embedded")]
    public required EmbeddedRecords Embedded { get; init; }

    /// <summary>
    ///     The list of records in this page.
    /// </summary>
    public List<T> Records => Embedded.Records;

    /// <summary>
    ///     Navigation links for this page.
    /// </summary>
    [JsonPropertyName("_links")]
    public PageLinks<T>? Links { get; init; }

    /// <summary>
    ///     The previous page of results or null when there is no more results
    /// </summary>
    /// <remarks>
    ///     The request goes through the <see cref="System.Net.Http.HttpClient" /> that fetched this page, so its
    ///     authentication, default headers and resilience pipeline apply to every page, and it can fail the same
    ///     ways the request for this page could. It goes to the scheme, host and port this page was fetched from,
    ///     whatever the link's href names, so a link cannot send the client's credentials to another host or over
    ///     plain HTTP. A redirect from that server is followed by the client's handler as for any other request,
    ///     with the client's default headers other than <c>Authorization</c>, which the .NET handlers drop on a
    ///     redirect; give the client a handler with <c>AllowAutoRedirect = false</c> to refuse redirects. A page
    ///     the SDK did not fetch (for example, one you deserialized yourself) is followed with a new client, to the
    ///     href as it is.
    /// </remarks>
    /// <exception cref="System.ObjectDisposedException">
    ///     Thrown by the returned task when the client that fetched this page has been disposed. A
    ///     <see cref="Server" /> disposes a client it created when it is disposed, so page through before
    ///     disposing it, or before disposing a client you passed to it or to <c>Link.Follow</c>.
    /// </exception>
    /// <exception cref="Exceptions.ClientProtocolException">
    ///     Thrown by the returned task when the response body is empty, or holds the JSON literal <c>null</c>.
    /// </exception>
    public Task<Page<T>>? PreviousPage()
    {
        return Follow(Links?.Prev);
    }

    /// <summary>
    ///     The next page of results or null when there is no more results
    /// </summary>
    /// <remarks>
    ///     The request goes through the <see cref="System.Net.Http.HttpClient" /> that fetched this page, so its
    ///     authentication, default headers and resilience pipeline apply to every page, and it can fail the same
    ///     ways the request for this page could. It goes to the scheme, host and port this page was fetched from,
    ///     whatever the link's href names, so a link cannot send the client's credentials to another host or over
    ///     plain HTTP. A redirect from that server is followed by the client's handler as for any other request,
    ///     with the client's default headers other than <c>Authorization</c>, which the .NET handlers drop on a
    ///     redirect; give the client a handler with <c>AllowAutoRedirect = false</c> to refuse redirects. A page
    ///     the SDK did not fetch (for example, one you deserialized yourself) is followed with a new client, to the
    ///     href as it is.
    /// </remarks>
    /// <exception cref="System.ObjectDisposedException">
    ///     Thrown by the returned task when the client that fetched this page has been disposed. A
    ///     <see cref="Server" /> disposes a client it created when it is disposed, so page through before
    ///     disposing it, or before disposing a client you passed to it or to <c>Link.Follow</c>.
    /// </exception>
    /// <exception cref="Exceptions.ClientProtocolException">
    ///     Thrown by the returned task when the response body is empty, or holds the JSON literal <c>null</c>.
    /// </exception>
    public Task<Page<T>>? NextPage()
    {
        return Follow(Links?.Next);
    }

    private Task<Page<T>>? Follow(Link<Page<T>>? link)
    {
        if (link is null)
        {
            return null;
        }

        return HttpClient is null || RequestUri is null
            ? link.Follow()
            : link.FollowOnOrigin(HttpClient, RequestUri);
    }

    /// <summary>
    ///     Container for embedded records in HAL JSON format.
    /// </summary>
    public sealed class EmbeddedRecords
    {
        /// <summary>
        ///     The list of records in this page.
        /// </summary>
        [JsonPropertyName("records")]
        public required List<T> Records { get; init; }
    }

    /// <summary>
    ///     Navigation links for paginated responses.
    /// </summary>
    public sealed class PageLinks<T>
    {
        /// <summary>
        ///     Link to the next page of results.
        /// </summary>
        public required Link<Page<T>> Next { get; init; }

        /// <summary>
        ///     Link to the previous page of results.
        /// </summary>
        public required Link<Page<T>> Prev { get; init; }

        /// <summary>
        ///     Link to this page.
        /// </summary>
        public required Link<Page<T>> Self { get; init; }
    }
}