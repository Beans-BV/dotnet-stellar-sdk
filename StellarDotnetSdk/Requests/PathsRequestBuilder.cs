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

    /// <summary>
    ///     Sets the destination account that any returned path should send to.
    /// </summary>
    /// <param name="account">The destination account ID (must be a valid Ed25519 public key).</param>
    /// <returns>The current <see cref="PathsRequestBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the account ID is invalid.</exception>
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

    /// <summary>
    ///     Sets the sender's account ID so the path search considers only assets held by this account.
    /// </summary>
    /// <param name="account">The source account ID (must be a valid Ed25519 public key).</param>
    /// <returns>The current <see cref="PathsRequestBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the account ID is invalid.</exception>
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

    /// <summary>
    ///     Sets the amount of the destination asset that should be received.
    /// </summary>
    /// <param name="amount">The desired destination amount.</param>
    /// <returns>The current <see cref="PathsRequestBuilder" /> instance for chaining.</returns>
    public PathsRequestBuilder DestinationAmount(string amount)
    {
        UriBuilder.SetQueryParam("destination_amount", amount);
        return this;
    }

    /// <summary>
    ///     Sets the destination asset that the recipient should receive.
    /// </summary>
    /// <param name="asset">The destination asset.</param>
    /// <returns>The current <see cref="PathsRequestBuilder" /> instance for chaining.</returns>
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