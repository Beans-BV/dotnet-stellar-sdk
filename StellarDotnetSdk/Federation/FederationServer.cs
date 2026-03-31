using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nett;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Federation;

/// <summary>
///     FederationServer handles a network connection to a
///     federation server (https://www.stellar.org/developers/learn/concepts/federation.html)
///     instance and exposes an interface for requests to that instance.
///     For resolving a stellar address without knowing which federation server
///     to query use Federation#resolve(String).
///     See Federation docs: https://www.stellar.org/developers/learn/concepts/federation.html
/// </summary>
public class FederationServer : IDisposable
{
    private static HttpClient _httpClient;

    /// <summary>
    ///     Initializes a new <see cref="FederationServer" /> with the given server URI and domain.
    ///     The URI must use HTTPS.
    /// </summary>
    /// <param name="serverUri">The URI of the federation server (must be HTTPS).</param>
    /// <param name="domain">The internet domain associated with this federation server.</param>
    public FederationServer(Uri serverUri, string domain)
    {
        if (serverUri.Scheme != "https")
        {
            throw new FederationServerInvalidException();
        }

        ServerUri = serverUri;

        if (Uri.CheckHostName(domain) == UriHostNameType.Unknown)
        {
            throw new ArgumentException("Invalid internet domain name supplied.", nameof(domain));
        }

        Domain = domain;
    }

    /// <summary>
    ///     Initializes a new <see cref="FederationServer" /> with the given server URI string and domain.
    /// </summary>
    /// <param name="serverUri">The URI string of the federation server (must be HTTPS).</param>
    /// <param name="domain">The internet domain associated with this federation server.</param>
    public FederationServer(string serverUri, string domain)
        : this(new Uri(serverUri), domain)
    {
    }

    /// <summary>Gets the URI of the federation server.</summary>
    public Uri ServerUri { get; }

    /// <summary>Gets the internet domain associated with this federation server.</summary>
    public string Domain { get; }

    /// <summary>Sets the <see cref="System.Net.Http.HttpClient" /> used for making requests to the federation server.</summary>
    public HttpClient HttpClient
    {
        set => _httpClient = value;
    }

    /// <summary>
    ///     Releases the resources used by the underlying <see cref="HttpClient" />.
    /// </summary>
    public void Dispose()
    {
        _httpClient.Dispose();
    }

    /// <summary>
    ///     Creates a <see cref="FederationServer" /> instance for a given domain.
    ///     It tries to find a federation server URL in stellar.toml file.
    ///     See: https://www.stellar.org/developers/learn/concepts/stellar-toml.html
    /// </summary>
    /// <param name="domain">Domain to find a federation server for</param>
    /// <returns>
    ///     <see cref="FederationServer" />
    /// </returns>
    public static async Task<FederationServer> CreateForDomain(string domain)
    {
        var uriBuilder = new StringBuilder();
        uriBuilder.Append("https://");
        uriBuilder.Append(domain);
        uriBuilder.Append("/.well-known/stellar.toml");
        var stellarTomUri = new Uri(uriBuilder.ToString());

        TomlTable stellarToml;
        try
        {
            var response = await _httpClient.GetAsync(stellarTomUri, HttpCompletionOption.ResponseContentRead);

            if (!response.IsSuccessStatusCode)
            {
                throw new StellarTomlNotFoundInvalidException();
            }

            var responseToml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            stellarToml = Toml.ReadString(responseToml);
        }
        catch (HttpRequestException e)
        {
            throw new ConnectionErrorException(e.Message);
        }

        var federationServer = stellarToml.Rows.Single(a => a.Key == "FEDERATION_SERVER").Value.Get<string>();
        if (string.IsNullOrWhiteSpace(federationServer))
        {
            throw new NoFederationServerException();
        }

        return new FederationServer(federationServer, domain);
    }

    /// <summary>
    ///     Resolves a Stellar federation address (e.g., <c>bob*stellar.org</c>) to a <see cref="FederationResponse" />.
    /// </summary>
    /// <param name="address">The Stellar federation address to resolve.</param>
    /// <returns>A <see cref="FederationResponse" /> containing the resolved account information.</returns>
    public async Task<FederationResponse> ResolveAddress(string address)
    {
        var tokens = Regex.Split(address, "\\*");
        if (tokens.Length != 2)
        {
            throw new MalformedAddressException();
        }

        var uriBuilder = new UriBuilder(ServerUri);
        uriBuilder.SetQueryParam("type", "name");
        uriBuilder.SetQueryParam("q", address);
        var uri = uriBuilder.Uri;

        try
        {
            var federationResponse = new ResponseHandler<FederationResponse>();

            var response = await _httpClient.GetAsync(uri);
            return await federationResponse.HandleResponse(response);
        }
        catch (HttpResponseException e)
        {
            if (e.StatusCode == 404)
            {
                throw new NotFoundException();
            }

            throw new ServerErrorException();
        }
        catch (Exception e)
        {
            throw new ConnectionErrorException(e.Message);
        }
    }
}