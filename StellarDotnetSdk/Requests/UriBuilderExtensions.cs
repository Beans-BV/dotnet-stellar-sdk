using System;
using System.Web;

namespace StellarDotnetSdk.Requests;

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
}