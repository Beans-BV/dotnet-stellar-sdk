using System;
using System.Net.Http;
using System.Threading.Tasks;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

public class FriendBotRequestBuilder : RequestBuilder<FriendBotRequestBuilder>
{
    /// <summary>
    /// </summary>
    /// <param name="serverUri"></param>
    public FriendBotRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "friendbot", httpClient)
    {
        if (Network.Current == null)
            throw new NotSupportedException("FriendBot requires the Testnet Network to be set explicitly.");

        if (Network.IsPublicNetwork(Network.Current))
            throw new NotSupportedException("FriendBot is only supported on the Testnet Network.");
    }

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