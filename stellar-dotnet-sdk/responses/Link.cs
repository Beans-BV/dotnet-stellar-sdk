using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using stellar_dotnet_sdk.requests;

namespace stellar_dotnet_sdk.responses;

public class Link
{
    public Link(string href)
    {
        Href = href;
    }

    [JsonProperty(PropertyName = "href")] public string Href { get; init; }

    [JsonIgnore] public virtual Uri Uri => new(Href);

    public virtual bool Templated => false;
}

[JsonConverter(typeof(LinkDeserializer))]
public class Link<TResponse> : Link
    where TResponse : Response
{
    public Link(string href) : base(href)
    {
    }

    public static Link<TResponse> Create(string href, bool templated)
    {
        if (templated) return new TemplatedLink<TResponse>(href);
        return new Link<TResponse>(href);
    }

    /// <summary>
    ///     Resolve template Uri by performing variables substitution.
    /// </summary>
    /// <returns></returns>
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
    /// <param name="httpClient"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public Task<TResponse> Follow(HttpClient httpClient, IDictionary<string, object> parameters)
    {
        return DoFollow(httpClient, Resolve(parameters));
    }

    /// <summary>
    ///     Follow the Link, retrieving the linked resource.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <returns></returns>
    public Task<TResponse> Follow(HttpClient httpClient)
    {
        return Follow(httpClient, null);
    }

    /// <summary>
    ///     Follow the Link, retrieving the linked resource.
    /// </summary>
    /// <returns></returns>
    public Task<TResponse> Follow()
    {
        return Follow(new HttpClient());
    }

    /// <summary>
    ///     Follow the Link, retrieving the linked resource.
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public Task<TResponse> Follow(object parameters)
    {
        return Follow(new HttpClient(), parameters);
    }

    /// <summary>
    ///     Follow the Link, retrieving the linked resource.
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
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

public class TemplatedLink<TResponse> : Link<TResponse>
    where TResponse : Response
{
    private UriTemplate? _uriTemplate;

    public TemplatedLink(string href) : base(href)
    {
        _uriTemplate = null;
    }

    public override Uri Uri => ParseUri()?.Resolve();

    public override bool Templated => true;

    public override Uri Resolve()
    {
        return ParseUri()?.Resolve();
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
        return ParseUri()?.Resolve(parameters);
    }

    /// <summary>
    ///     Resolve template Uri by performing variables substitution.
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public override Uri Resolve(IDictionary<string, object> parameters)
    {
        return ParseUri()?.Resolve(parameters);
    }


    private UriTemplate ParseUri()
    {
        if (_uriTemplate is null) _uriTemplate = new UriTemplate(Href);
        return _uriTemplate;
    }
}