using System;
using System.Net.Http;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Builds requests connected to paths.
/// </summary>
public class PathsRequestBuilder : RequestBuilderExecutePageable<PathsRequestBuilder, PathResponse>
{
    public PathsRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "paths", httpClient)
    {
    }

    public PathsRequestBuilder DestinationAccount(string account)
    {
        ArgumentException.ThrowIfNullOrEmpty(account);

        if (!StrKey.IsValidEd25519PublicKey(account))
        {
            throw new ArgumentException($"Invalid account ID {account}");
        }
        UriBuilder.SetQueryParam("destination_account", account);
        return this;
    }

    public PathsRequestBuilder SourceAccount(string account)
    {
        ArgumentException.ThrowIfNullOrEmpty(account);

        if (!StrKey.IsValidEd25519PublicKey(account))
        {
            throw new ArgumentException($"Invalid account ID {account}");
        }
        UriBuilder.SetQueryParam("source_account", account);
        return this;
    }

    public PathsRequestBuilder DestinationAmount(string amount)
    {
        UriBuilder.SetQueryParam("destination_amount", amount);
        return this;
    }

    public PathsRequestBuilder DestinationAsset(Asset asset)
    {
        UriBuilder.SetQueryParam("destination_asset_type", asset.Type);

        if (asset is AssetTypeCreditAlphaNum creditAlphaNumAsset)
        {
            UriBuilder.SetQueryParam("destination_asset_code", creditAlphaNumAsset.Code);
            UriBuilder.SetQueryParam("destination_asset_issuer", creditAlphaNumAsset.Issuer);
        }

        return this;
    }
}