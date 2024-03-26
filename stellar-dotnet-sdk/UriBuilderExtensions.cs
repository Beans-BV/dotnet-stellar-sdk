using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace stellar_dotnet_sdk;

public static class UriBuilderExtensions
{
    /// <summary>
    ///     Add or replace the specified query parameter key-value pair of the URI.
    /// </summary>
    /// <param name="uriBuilder">The URI builder.</param>
    /// <param name="key">The key to remove.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>The <see cref="UriBuilder" /> with the key-value pair set.</returns>
    public static UriBuilder SetQueryParam(this UriBuilder uriBuilder, string key, string value)
    {
        var queryParameters = HttpUtility.ParseQueryString(uriBuilder.Query);

        queryParameters[key] = value;

        uriBuilder.Query = queryParameters.ToString();

        return uriBuilder;
    }

    /// <summary>
    ///     Removes the specified query parameter key-value pair of the URI.
    /// </summary>
    /// <param name="uriBuilder">The URI builder.</param>
    /// <param name="key">The key to remove.</param>
    /// <returns>The <see cref="UriBuilder" /> with the key removed.</returns>
    public static UriBuilder RemoveQueryParam(this UriBuilder uriBuilder, string key)
    {
        var queryParameters = HttpUtility.ParseQueryString(uriBuilder.Query);

        queryParameters.Remove(key);

        uriBuilder.Query = queryParameters.ToString();

        return uriBuilder;
    }

    public static UriBuilder SetPath(this UriBuilder uri, string path)
    {
        uri.Path = path.StartsWith("/") ? path : $"/{path}";
        return uri;
    }

    /// <summary>
    ///     Gets the query string key-value pairs of the URI.
    ///     Note that the one of the keys may be null ("?123") and
    ///     that one of the keys may be an empty string ("?=123").
    /// </summary>
    public static IEnumerable<KeyValuePair<string, string>> GetQueryParams(
        this UriBuilder uri)
    {
        return uri.ParseQuery().AsKeyValuePairs();
    }

    /// <summary>
    ///     Converts the legacy NameValueCollection into a strongly-typed KeyValuePair sequence.
    /// </summary>
    private static IEnumerable<KeyValuePair<string, string>> AsKeyValuePairs(this NameValueCollection collection)
    {
        foreach (var key in collection.AllKeys)
            yield return new KeyValuePair<string, string>(key, collection.Get(key));
    }

    /// <summary>
    ///     Parses the query string of the URI into a NameValueCollection.
    /// </summary>
    private static NameValueCollection ParseQuery(this UriBuilder uri)
    {
        return HttpUtility.ParseQueryString(uri.Query);
    }
}