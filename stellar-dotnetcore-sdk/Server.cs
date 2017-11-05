﻿using System;
using System.Net.Http;
using stellar_dotnetcore_sdk.requests;

namespace stellar_dotnetcore_sdk
{
    public class Server
    {
        private readonly Uri _serverUri;
        private HttpClient _httpClient;

        public Server(string uri)
        {
            _httpClient = new HttpClient();

            try
            {
                _serverUri = new Uri(uri);
            }
            catch (UriFormatException)
            {
                throw;
            }
        }

        public AccountsRequestBuilder Accounts => new AccountsRequestBuilder(_serverUri);

        public EffectsRequestBuilder Effects => new EffectsRequestBuilder(_serverUri);

        public LedgersRequestBuilder Ledgers => new LedgersRequestBuilder(_serverUri);

        public OffersRequestBuilder Offers => new OffersRequestBuilder(_serverUri);

        public OrderBookRequestBuilder OrderBook => new OrderBookRequestBuilder(_serverUri);

        public PathsRequestBuilder Paths => new PathsRequestBuilder(_serverUri);

        //TODO: Implement the rest of this class, has many many dependencies...
    }
}