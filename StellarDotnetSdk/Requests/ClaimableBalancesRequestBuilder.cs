using System;
using System.Net.Http;
using System.Threading.Tasks;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Builds requests to the Horizon <c>/claimable_balances</c> endpoint for retrieving
///     claimable balance details and listing claimable balances.
/// </summary>
public class
    ClaimableBalancesRequestBuilder : RequestBuilderExecutePageable<ClaimableBalancesRequestBuilder,
    ClaimableBalanceResponse>
{
    /// <summary>
    ///     Initializes a new <see cref="ClaimableBalancesRequestBuilder" />.
    /// </summary>
    /// <param name="serverUri">The base Horizon server URI.</param>
    /// <param name="httpClient">The HTTP client used for sending requests.</param>
    public ClaimableBalancesRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "claimable_balances", httpClient)
    {
    }


    /// <summary>
    ///     Filters claimable balances by the given asset.
    /// </summary>
    /// <param name="asset">The asset to filter claimable balances by.</param>
    /// <returns>The current <see cref="ClaimableBalancesRequestBuilder" /> instance for chaining.</returns>
    public ClaimableBalancesRequestBuilder ForAsset(Asset asset)
    {
        UriBuilder.SetQueryParam("asset", asset.CanonicalName());
        return this;
    }

    /// <summary>
    ///     Filters claimable balances by the given claimant account.
    /// </summary>
    /// <param name="claimant">The claimant account to filter by.</param>
    /// <returns>The current <see cref="ClaimableBalancesRequestBuilder" /> instance for chaining.</returns>
    public ClaimableBalancesRequestBuilder ForClaimant(KeyPair claimant)
    {
        UriBuilder.SetQueryParam("claimant", claimant.Address);
        return this;
    }

    /// <summary>
    ///     Filters claimable balances by the given sponsor account.
    /// </summary>
    /// <param name="sponsor">The sponsor account to filter by.</param>
    /// <returns>The current <see cref="ClaimableBalancesRequestBuilder" /> instance for chaining.</returns>
    public ClaimableBalancesRequestBuilder ForSponsor(KeyPair sponsor)
    {
        UriBuilder.SetQueryParam("sponsor", sponsor.Address);
        return this;
    }

    /// <summary>
    ///     Requests a single claimable balance resource from the specified URI.
    /// </summary>
    /// <param name="uri">The URI of the claimable balance resource.</param>
    /// <returns>The <see cref="ClaimableBalanceResponse" />.</returns>
    public async Task<ClaimableBalanceResponse> ClaimableBalance(Uri uri)
    {
        var responseHandler = new ResponseHandler<ClaimableBalanceResponse>();
        var response = await HttpClient.GetAsync(uri);
        return await responseHandler.HandleResponse(response);
    }

    /// <summary>
    ///     Requests <c>GET /claimable_balances/{balanceId}</c>.
    /// </summary>
    /// <param name="balanceId">The ID of the claimable balance to fetch.</param>
    /// <returns>The <see cref="ClaimableBalanceResponse" />.</returns>
    public Task<ClaimableBalanceResponse> ClaimableBalance(string balanceId)
    {
        SetSegments("claimable_balances", balanceId);
        return ClaimableBalance(BuildUri());
    }
}