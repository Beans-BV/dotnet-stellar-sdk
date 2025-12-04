using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Responses;

public class Link
{
    [JsonPropertyName("href")]
    public string Href { get; init; }

    [JsonIgnore]
    public virtual Uri Uri => new(Href);

    public virtual bool Templated => false;
}

public class Link<TResponse> : Link where TResponse : Response
{
    public static Link<TResponse> Create(string href, bool templated)
    {
        return templated
            ? new TemplatedLink<TResponse> { Href = href }
            : new Link<TResponse> { Href = href };
    }

    /// <summary>
    ///     Resolve template Uri by performing variables substitution.
    /// </summary>
    public virtual Uri Resolve()
    {
        return Uri;
    }

    public virtual Uri Resolve(object parameters)
    {
        return Uri;
    }

    public virtual Uri Resolve(IDictionary<string, object> parameters)
    {
        return Uri;
    }

    public Task<TResponse> Follow(HttpClient httpClient, object parameters)
    {
        return DoFollow(httpClient, Resolve(parameters));
    }

    /// <summary>
    ///     Follow the Link, retrieving the linked resource.
    /// </summary>
    public Task<TResponse> Follow(HttpClient httpClient, IDictionary<string, object> parameters)
    {
        return DoFollow(httpClient, Resolve(parameters));
    }

    /// <summary>
    ///     Follow the Link, retrieving the linked resource.
    /// </summary>
    public Task<TResponse> Follow(HttpClient httpClient)
    {
        return Follow(httpClient, null);
    }

    /// <summary>
    ///     Follow the Link, retrieving the linked resource.
    /// </summary>
    public Task<TResponse> Follow()
    {
        return Follow(new HttpClient());
    }

    /// <summary>
    ///     Follow the Link, retrieving the linked resource.
    /// </summary>
    public Task<TResponse> Follow(object parameters)
    {
        return Follow(new HttpClient(), parameters);
    }

    /// <summary>
    ///     Follow the Link, retrieving the linked resource.
    /// </summary>
    public Task<TResponse> Follow(IDictionary<string, object> parameters)
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

public class TemplatedLink<TResponse> : Link<TResponse> where TResponse : Response
{
    private UriTemplate? _uriTemplate;

    public override Uri Uri => ParseUri().Resolve();

    public override bool Templated => true;

    public override Uri Resolve()
    {
        return ParseUri().Resolve();
    }

    /// <summary>
    ///     Resolve template Uri by performing variables substitution.
    /// </summary>
    /// <param name="parameters"></param>
    /// <example>
    ///     <code>
    /// var uri = link.Resolve(new {
    ///     limit = 10,
    ///     order = OrderDirection.DESC
    /// });
    /// </code>
    /// </example>
    /// <returns></returns>
    public override Uri Resolve(object parameters)
    {
        return ParseUri().Resolve(parameters);
    }

    /// <summary>
    ///     Resolve template Uri by performing variables substitution.
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public override Uri Resolve(IDictionary<string, object> parameters)
    {
        return ParseUri().Resolve(parameters);
    }

    private UriTemplate ParseUri()
    {
        return _uriTemplate ??= new UriTemplate(Href);
    }
}