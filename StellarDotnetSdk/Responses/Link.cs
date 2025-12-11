using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a HAL hypermedia link.
///     Links provide navigation to related resources in the Horizon API.
/// </summary>
public class Link
{
    /// <summary>
    ///     The URL of the linked resource.
    /// </summary>
    [JsonPropertyName("href")]
    public required string Href { get; init; }

    /// <summary>
    ///     The URI representation of the link's href.
    /// </summary>
    [JsonIgnore]
    public virtual Uri Uri => new(Href);

    /// <summary>
    ///     Whether this link is a URI template that requires variable substitution.
    /// </summary>
    public virtual bool Templated => false;
}

/// <summary>
///     Represents a typed HAL hypermedia link that can be followed to retrieve a resource.
///     Supports both direct links and URI templates.
/// </summary>
/// <typeparam name="TResponse">The type of response expected when following this link.</typeparam>
public class Link<TResponse> : Link where TResponse : Response
{
    /// <summary>
    ///     Creates a Link or TemplatedLink based on the templated flag.
    /// </summary>
    /// <param name="href">The URL or URI template.</param>
    /// <param name="templated">Whether the href is a URI template.</param>
    /// <returns>A Link or TemplatedLink instance.</returns>
    public static Link<TResponse> Create(string href, bool templated)
    {
        return templated
            ? new TemplatedLink<TResponse> { Href = href }
            : new Link<TResponse> { Href = href };
    }

    /// <summary>
    ///     Resolves the link URI. For non-templated links, returns the URI directly.
    /// </summary>
    /// <returns>The resolved URI.</returns>
    public virtual Uri Resolve()
    {
        return Uri;
    }

    /// <summary>
    ///     Resolves the link URI with the given parameters.
    ///     For non-templated links, parameters are ignored.
    /// </summary>
    /// <param name="parameters">An object whose properties are used for template substitution.</param>
    /// <returns>The resolved URI.</returns>
    public virtual Uri Resolve(object? parameters)
    {
        return Uri;
    }

    /// <summary>
    ///     Resolves the link URI with the given parameters.
    ///     For non-templated links, parameters are ignored.
    /// </summary>
    /// <param name="parameters">A dictionary of parameter names and values for template substitution.</param>
    /// <returns>The resolved URI.</returns>
    public virtual Uri Resolve(IDictionary<string, object>? parameters)
    {
        return Uri;
    }

    /// <summary>
    ///     Follows the link to retrieve the linked resource.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="parameters">An object whose properties are used for template substitution.</param>
    /// <returns>The linked resource.</returns>
    public Task<TResponse> Follow(HttpClient httpClient, object? parameters)
    {
        return DoFollow(httpClient, Resolve(parameters));
    }

    /// <summary>
    ///     Follows the link to retrieve the linked resource.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="parameters">A dictionary of parameter names and values for template substitution.</param>
    /// <returns>The linked resource.</returns>
    public Task<TResponse> Follow(HttpClient httpClient, IDictionary<string, object>? parameters)
    {
        return DoFollow(httpClient, Resolve(parameters));
    }

    /// <summary>
    ///     Follows the link to retrieve the linked resource.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <returns>The linked resource.</returns>
    public Task<TResponse> Follow(HttpClient httpClient)
    {
        return Follow(httpClient, null);
    }

    /// <summary>
    ///     Follows the link to retrieve the linked resource using a new HttpClient.
    /// </summary>
    /// <returns>The linked resource.</returns>
    public Task<TResponse> Follow()
    {
        return Follow(new HttpClient());
    }

    /// <summary>
    ///     Follows the link to retrieve the linked resource using a new HttpClient.
    /// </summary>
    /// <param name="parameters">An object whose properties are used for template substitution.</param>
    /// <returns>The linked resource.</returns>
    public Task<TResponse> Follow(object? parameters)
    {
        return Follow(new HttpClient(), parameters);
    }

    /// <summary>
    ///     Follows the link to retrieve the linked resource using a new HttpClient.
    /// </summary>
    /// <param name="parameters">A dictionary of parameter names and values for template substitution.</param>
    /// <returns>The linked resource.</returns>
    public Task<TResponse> Follow(IDictionary<string, object>? parameters)
    {
        return Follow(new HttpClient(), parameters);
    }

    private async Task<TResponse> DoFollow(HttpClient httpClient, Uri uri)
    {
        var responseHandler = new ResponseHandler<TResponse>();
        var response = await httpClient.GetAsync(uri);
        return await responseHandler.HandleResponse(response);
    }
}

/// <summary>
///     Represents a templated HAL hypermedia link that requires variable substitution.
///     URI templates follow RFC 6570.
/// </summary>
/// <typeparam name="TResponse">The type of response expected when following this link.</typeparam>
public sealed class TemplatedLink<TResponse> : Link<TResponse>
    where TResponse : Response
{
    private UriTemplate? _uriTemplate;

    /// <summary>
    ///     The URI with all template variables resolved to empty values.
    /// </summary>
    public override Uri Uri => ParseUri().Resolve();

    /// <summary>
    ///     Returns true to indicate this is a templated link.
    /// </summary>
    public override bool Templated => true;

    /// <summary>
    ///     Resolves the URI template with no variable substitution.
    /// </summary>
    /// <returns>The resolved URI.</returns>
    public override Uri Resolve()
    {
        return ParseUri().Resolve();
    }

    /// <summary>
    ///     Resolves the URI template by substituting variables from the given object's properties.
    /// </summary>
    /// <param name="parameters">An object whose properties are used for template substitution.</param>
    /// <example>
    ///     <code>
    /// var uri = link.Resolve(new {
    ///     limit = 10,
    ///     order = OrderDirection.DESC
    /// });
    /// </code>
    /// </example>
    /// <returns>The resolved URI.</returns>
    public override Uri Resolve(object? parameters)
    {
        return parameters != null ? ParseUri().Resolve(parameters) : ParseUri().Resolve();
    }

    /// <summary>
    ///     Resolves the URI template by substituting variables from the given dictionary.
    /// </summary>
    /// <param name="parameters">A dictionary of parameter names and values for template substitution.</param>
    /// <returns>The resolved URI.</returns>
    public override Uri Resolve(IDictionary<string, object>? parameters)
    {
        return parameters != null ? ParseUri().Resolve(parameters) : ParseUri().Resolve();
    }

    private UriTemplate ParseUri()
    {
        return _uriTemplate ??= new UriTemplate(Href);
    }
}