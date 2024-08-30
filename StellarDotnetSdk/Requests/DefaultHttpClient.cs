using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace StellarDotnetSdk.Requests;

internal class DefaultHttpClient : HttpClient
{
    /// <summary>
    ///     Creates an HTTP client with some default request headers and the given bearer token.
    /// </summary>
    /// <param name="bearerToken">Bearer token in case the server requires it.</param>
    public DefaultHttpClient(string? bearerToken = null)
    {
        var assembly = Assembly.GetAssembly(GetType())!.GetName();
        DefaultRequestHeaders.Add("X-Client-Name", "stellar-dotnet-sdk");
        DefaultRequestHeaders.Add("X-Client-Version", assembly.Version!.ToString());
        if (!string.IsNullOrEmpty(bearerToken))
        {
            DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
    }
}