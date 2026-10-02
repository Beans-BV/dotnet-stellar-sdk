using System;
using System.Net.Http;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

/// <summary>
/// </summary>
public class AssetsRequestBuilder : RequestBuilderExecutePageable<AssetsRequestBuilder, AssetResponse>
{
    /// <summary>
    ///     Initializes a new <see cref="AssetsRequestBuilder" />.
    /// </summary>
    /// <param name="serverUri">The base Horizon server URI.</param>
    /// <param name="httpClient">The HTTP client used for sending requests.</param>
    public AssetsRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "assets", httpClient)
    {
    }

    /// <summary>
    ///     Code of the Asset to filter by
    /// </summary>
    /// <param name="assetCode"></param>
    /// <returns></returns>
    public AssetsRequestBuilder AssetCode(string assetCode)
    {
        UriBuilder.SetQueryParam("asset_code", assetCode);
        return this;
    }

    /// <summary>
    ///     Issuer of the Asset to filter by
    /// </summary>
    /// <param name="assetIssuer"></param>
    /// <returns></returns>
    public AssetsRequestBuilder AssetIssuer(string assetIssuer)
    {
        UriBuilder.SetQueryParam("asset_issuer", assetIssuer);
        return this;
    }
}