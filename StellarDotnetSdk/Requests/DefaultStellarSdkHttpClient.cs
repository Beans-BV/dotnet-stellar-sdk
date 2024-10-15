using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace StellarDotnetSdk.Requests;

public class DefaultStellarSdkHttpClient : HttpClient
{
    /// <summary>
    ///     Creates an HTTP client with some default request headers and the given bearer token.
    /// </summary>
    /// <param name="bearerToken">Bearer token in case the server requires it.</param>
    /// <param name="clientName">Name of the client.</param>
    /// <param name="clientVersion">Version of the client.</param>
    public DefaultStellarSdkHttpClient(
        string? bearerToken = null,
        string? clientName = null,
        string? clientVersion = null)
    {
        var assembly = Assembly.GetAssembly(GetType())!.GetName();
        DefaultRequestHeaders.Add("X-Client-Name", clientName ?? "stellar-dotnet-sdk");
        DefaultRequestHeaders.Add("X-Client-Version", clientVersion ?? assembly.Version!.ToString());
        if (!string.IsNullOrEmpty(bearerToken))
        {
            DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
    }
}