using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

public class
    PathStrictSendRequestBuilder : RequestBuilderExecutePageable<PathStrictSendRequestBuilder, PathResponse>
{
    public PathStrictSendRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "paths/strict-send", httpClient)
    {
    }

    /// <summary>
    ///     Sets the destination account for the path payment.
    /// </summary>
    /// <param name="account">The destination account ID.</param>
    /// <returns>The current <see cref="PathStrictSendRequestBuilder" /> instance for chaining.</returns>
    public PathStrictSendRequestBuilder DestinationAccount(string account)
    {
        UriBuilder.SetQueryParam("destination_account", account);
        return this;
    }

    /// <summary>
    ///     Sets the amount of the source asset to be sent.
    /// </summary>
    /// <param name="amount">The source amount.</param>
    /// <returns>The current <see cref="PathStrictSendRequestBuilder" /> instance for chaining.</returns>
    public PathStrictSendRequestBuilder SourceAmount(string amount)
    {
        UriBuilder.SetQueryParam("source_amount", amount);
        return this;
    }

    /// <summary>
    ///     Sets the source asset to be sent in the path payment.
    /// </summary>
    /// <param name="asset">The source asset.</param>
    /// <returns>The current <see cref="PathStrictSendRequestBuilder" /> instance for chaining.</returns>
    public PathStrictSendRequestBuilder SourceAsset(Asset asset)
    {
        UriBuilder.SetQueryParam("source_asset_type", asset.Type);

        if (asset is not AssetTypeCreditAlphaNum creditAlphaNumAsset)
        {
            return this;
        }
        UriBuilder.SetQueryParam("source_asset_code", creditAlphaNumAsset.Code);
        UriBuilder.SetQueryParam("source_asset_issuer", creditAlphaNumAsset.Issuer);

        return this;
    }

    /// <summary>
    ///     Sets the list of destination assets to consider for the path payment.
    /// </summary>
    /// <param name="destinationAssets">The destination assets to consider.</param>
    /// <returns>The current <see cref="PathStrictSendRequestBuilder" /> instance for chaining.</returns>
    public PathStrictSendRequestBuilder DestinationAssets(IEnumerable<Asset> destinationAssets)
    {
        var encodedAssets = destinationAssets.Select(a => a.ToQueryParameterEncodedString());
        UriBuilder.SetQueryParam("destination_assets", string.Join(",", encodedAssets));
        return this;
    }
}