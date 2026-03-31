using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Builds requests to the Horizon <c>/paths/strict-receive</c> endpoint for finding
///     payment paths that deliver a specified amount of a destination asset.
/// </summary>
public class
    PathStrictReceiveRequestBuilder : RequestBuilderExecutePageable<PathStrictReceiveRequestBuilder, PathResponse>
{
    /// <summary>
    ///     Initializes a new <see cref="PathStrictReceiveRequestBuilder" />.
    /// </summary>
    /// <param name="serverUri">The base Horizon server URI.</param>
    /// <param name="httpClient">The HTTP client used for sending requests.</param>
    public PathStrictReceiveRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "paths/strict-receive", httpClient)
    {
    }

    /// <summary>
    ///     Sets the source account for the path payment. Cannot be used together with <see cref="SourceAssets" />.
    /// </summary>
    /// <param name="account">The source account ID.</param>
    /// <returns>The current <see cref="PathStrictReceiveRequestBuilder" /> instance for chaining.</returns>
    public PathStrictReceiveRequestBuilder SourceAccount(string account)
    {
        UriBuilder.SetQueryParam("source_account", account);
        return this;
    }

    /// <summary>
    ///     Sets the destination account for the path payment.
    /// </summary>
    /// <param name="account">The destination account ID.</param>
    /// <returns>The current <see cref="PathStrictReceiveRequestBuilder" /> instance for chaining.</returns>
    public PathStrictReceiveRequestBuilder DestinationAccount(string account)
    {
        UriBuilder.SetQueryParam("destination_account", account);
        return this;
    }

    /// <summary>
    ///     Sets the amount of the destination asset that should be received.
    /// </summary>
    /// <param name="amount">The destination amount.</param>
    /// <returns>The current <see cref="PathStrictReceiveRequestBuilder" /> instance for chaining.</returns>
    public PathStrictReceiveRequestBuilder DestinationAmount(string amount)
    {
        UriBuilder.SetQueryParam("destination_amount", amount);
        return this;
    }

    /// <summary>
    ///     Sets the destination asset to be received in the path payment.
    /// </summary>
    /// <param name="asset">The destination asset.</param>
    /// <returns>The current <see cref="PathStrictReceiveRequestBuilder" /> instance for chaining.</returns>
    public PathStrictReceiveRequestBuilder DestinationAsset(Asset asset)
    {
        UriBuilder.SetQueryParam("destination_asset_type", asset.Type);

        if (asset is AssetTypeCreditAlphaNum)
        {
            var creditAlphaNumAsset = (AssetTypeCreditAlphaNum)asset;
            UriBuilder.SetQueryParam("destination_asset_code", creditAlphaNumAsset.Code);
            UriBuilder.SetQueryParam("destination_asset_issuer", creditAlphaNumAsset.Issuer);
        }

        return this;
    }

    /// <summary>
    ///     Sets the list of source assets to consider for the path payment. Cannot be used together with
    ///     <see cref="SourceAccount" />.
    /// </summary>
    /// <param name="sources">The source assets to consider.</param>
    /// <returns>The current <see cref="PathStrictReceiveRequestBuilder" /> instance for chaining.</returns>
    public PathStrictReceiveRequestBuilder SourceAssets(IEnumerable<Asset> sources)
    {
        var encodedAssets = sources.Select(a => a.ToQueryParameterEncodedString());
        UriBuilder.SetQueryParam("source_assets", string.Join(",", encodedAssets));
        return this;
    }
}