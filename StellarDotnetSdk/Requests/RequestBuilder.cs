using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Specifies the ordering direction for paginated Horizon responses.
/// </summary>
public enum OrderDirection
{
    /// <summary>Ascending order (oldest first).</summary>
    ASC,

    /// <summary>Descending order (newest first).</summary>
    DESC,
}

/// <summary>
///     Abstract class for request builders.
/// </summary>
public interface IRequestBuilder<T> where T : class
{
    /// <summary>
    ///     Gets the fully resolved request URI as a string.
    /// </summary>
    string Uri { get; }

    /// <summary>
    ///     Executes an HTTP GET request to the specified URI and deserializes the response.
    /// </summary>
    /// <typeparam name="TZ">The response type to deserialize into.</typeparam>
    /// <param name="uri">The URI to send the request to.</param>
    /// <returns>The deserialized response object.</returns>
    Task<TZ> Execute<TZ>(Uri uri) where TZ : class;

    /// <summary>
    ///     Sets <code>cursor</code> parameter on the request.
    ///     A cursor is a value that points to a specific location in a collection of resources.
    ///     The cursor attribute itself is an opaque value meaning that users should not try to parse it.
    ///     Read https://www.stellar.org/developers/horizon/reference/resources/page.html for more information.
    /// </summary>
    /// <param name="cursor"></param>
    /// <returns></returns>
    T Cursor(string cursor);

    /// <summary>
    ///     Sets <code>limit</code> parameter on the request.
    ///     It defines maximum number of records to return.
    ///     For range and default values check documentation of the endpoint requested.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    T Limit(int number);

    /// <summary>
    ///     Sets order parameter on request.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    T Order(OrderDirection direction);

    /// <summary>
    ///     Allows to stream SSE events from horizon.
    ///     Certain endpoints in Horizon can be called in streaming mode using Server-Sent Events.
    ///     This mode will keep the connection to horizon open and horizon will continue to return
    ///     http://www.w3.org/TR/eventsource/
    ///     "https://www.stellar.org/developers/horizon/learn/responses.html
    ///     responses as ledgers close.
    /// </summary>
    /// <param name="listener">EventListener implementation with AccountResponse type</param>
    /// <returns>EventSource object, so you can close() connection when not needed anymore</returns>
    Uri BuildUri();
}

/// <summary>
///     Abstract class for request builders.
/// </summary>
public class RequestBuilder<T> : IRequestBuilder<T> where T : class
{
    private readonly List<string> _segments;
    private readonly Uri _serverUri;
    /// <summary>
    ///     The URI builder used to construct the request URL with path segments and query parameters.
    /// </summary>
    protected readonly UriBuilder UriBuilder;
    private bool _segmentsAdded;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequestBuilder{T}" /> class.
    /// </summary>
    /// <param name="serverUri">The base Horizon server URI.</param>
    /// <param name="defaultSegment">The default URL path segment for this endpoint.</param>
    /// <param name="httpClient">The HTTP client used for sending requests.</param>
    public RequestBuilder(Uri serverUri, string defaultSegment, HttpClient httpClient)
    {
        _serverUri = serverUri;

        UriBuilder = new UriBuilder(serverUri);
        _segments = new List<string>();

        if (!string.IsNullOrEmpty(defaultSegment))
        {
            SetSegments(defaultSegment);
        }

        _segmentsAdded = false; //Allow overwriting segments
        HttpClient = httpClient;
    }

    /// <summary>
    ///     Gets or sets the HTTP client shared across all request builder instances.
    /// </summary>
    public static HttpClient HttpClient { get; set; }

    /// <inheritdoc />
    public async Task<TZ> Execute<TZ>(Uri uri) where TZ : class
    {
        var responseHandler = new ResponseHandler<TZ>();

        var response = await HttpClient.GetAsync(uri);
        return await responseHandler.HandleResponse(response);
    }

    /// <summary>
    ///     Gets the fully resolved request URI as a string.
    /// </summary>
    public string Uri => BuildUri().ToString();

    /// <summary>
    ///     Sets <code>cursor</code> parameter on the request.
    ///     A cursor is a value that points to a specific location in a collection of resources.
    ///     The cursor attribute itself is an opaque value meaning that users should not try to parse it.
    ///     Read https://www.stellar.org/developers/horizon/reference/resources/page.html for more information.
    /// </summary>
    /// <param name="cursor"></param>
    /// <returns></returns>
    public virtual T Cursor(string cursor)
    {
        UriBuilder.SetQueryParam("cursor", cursor);

        return this as T;
    }

    /// <summary>
    ///     Sets <code>limit</code> parameter on the request.
    ///     It defines maximum number of records to return.
    ///     For range and default values check documentation of the endpoint requested.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public virtual T Limit(int number)
    {
        UriBuilder.SetQueryParam("limit", number.ToString());

        return this as T;
    }

    /// <summary>
    ///     Sets order parameter on request.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public virtual T Order(OrderDirection direction)
    {
        UriBuilder.SetQueryParam("order", direction.ToString().ToLower());

        return this as T;
    }

    /// <summary>
    ///     Allows to stream SSE events from horizon.
    ///     Certain endpoints in Horizon can be called in streaming mode using Server-Sent Events.
    ///     This mode will keep the connection to horizon open and horizon will continue to return
    ///     http://www.w3.org/TR/eventsource/
    ///     "https://www.stellar.org/developers/horizon/learn/responses.html
    ///     responses as ledgers close.
    /// </summary>
    /// <param name="listener">
    ///     EventListener implementation with AccountResponse type
    /// </param>
    /// <returns>EventSource object, so you can close() connection when not needed anymore</returns>
    public Uri BuildUri()
    {
        if (_segments.Count <= 0)
        {
            throw new NotSupportedException("No segments defined.");
        }

        var path = _serverUri.AbsolutePath.TrimEnd('/');

        foreach (var segment in _segments)
        {
            if (!path.EndsWith("/"))
            {
                path += "/";
            }
            path += segment;
        }

        UriBuilder.Path = path;

        return UriBuilder.Uri;
    }

    /// <summary>
    ///     Sets the URL path segments for this request, replacing any default segment.
    /// </summary>
    /// <param name="segments">The path segments to set.</param>
    /// <returns>The current <see cref="RequestBuilder{T}" /> instance.</returns>
    /// <exception cref="Exception">Thrown if segments have already been added.</exception>
    protected RequestBuilder<T> SetSegments(params string[] segments)
    {
        if (_segmentsAdded)
        {
            throw new Exception("URL segments have been already added.");
        }

        _segmentsAdded = true;

        //Remove default segments
        _segments.Clear();

        foreach (var segment in segments)
        {
            _segments.Add(segment);
        }

        return this;
    }
}