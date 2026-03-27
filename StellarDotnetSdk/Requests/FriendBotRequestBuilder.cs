using System;
using System.Net.Http;
using System.Threading.Tasks;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Builds requests to the Stellar Testnet Friendbot service, which funds accounts
///     with test Lumens. Only available on the Testnet network.
/// </summary>
public class FriendBotRequestBuilder : RequestBuilder<FriendBotRequestBuilder>
{
    /// <summary>
    /// </summary>
    /// <param name="serverUri"></param>
    public FriendBotRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "friendbot", httpClient)
    {
        if (Network.Current == null)
        {
            throw new NotSupportedException("FriendBot requires the Testnet Network to be set explicitly.");
        }

        if (Network.IsPublicNetwork(Network.Current))
        {
            throw new NotSupportedException("FriendBot is only supported on the Testnet Network.");
        }
    }

    /// <summary>
    ///     Sets the account to be funded by Friendbot.
    /// </summary>
    /// <param name="accountId">The public key of the account to fund with test Lumens.</param>
    /// <returns>The current <see cref="FriendBotRequestBuilder" /> instance for chaining.</returns>
    public FriendBotRequestBuilder FundAccount(string accountId)
    {
        UriBuilder.SetQueryParam("addr", accountId);
        return this;
    }

    /// <Summary>
    ///     Build and execute request.
    /// </Summary>
    public async Task<FriendBotResponse> Execute()
    {
        return await Execute<FriendBotResponse>(BuildUri());
    }
}