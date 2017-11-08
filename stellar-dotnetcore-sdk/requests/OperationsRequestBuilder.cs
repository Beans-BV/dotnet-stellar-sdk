﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using stellar_dotnetcore_sdk.responses.operations;
using stellar_dotnetcore_sdk.responses.page;

namespace stellar_dotnetcore_sdk.requests
{
    public class OperationsRequestBuilder : RequestBuilder<OperationsRequestBuilder>
    {
        /// <summary>
        ///     Builds requests connected to operations.
        /// </summary>
        /// <param name="serverUri"></param>
        public OperationsRequestBuilder(Uri serverUri)
            : base(serverUri, "operations")
        {
        }

        /// <summary>
        ///     Requests specific uri and returns <see cref="OperationResponse" />.
        ///     This method is helpful for getting the links.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>
        ///     <see cref="Task{OperationResponse}" />
        /// </returns>
        public async Task<OperationResponse> Operation(Uri uri)
        {
            var responseHandler = new ResponseHandler<OperationResponse>();
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(uri);
                return await responseHandler.HandleResponse(response);
            }
        }

        /// <summary>
        ///     Requests GET /operations/{operationId}
        ///     See: https://www.stellar.org/developers/horizon/reference/operations-single.html
        /// </summary>
        /// <param name="operationId">Operation to fetch</param>
        /// <returns>
        ///     <see cref="Task{OperationResponse}" />
        /// </returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<OperationResponse> Operation(long operationId)
        {
            SetSegments("operation", operationId.ToString());
            return await Operation(BuildUri());
        }

        /// <summary>
        ///     Builds request to GET /accounts/{account}/operations
        ///     See: https://www.stellar.org/developers/horizon/reference/operations-for-account.html
        /// </summary>
        /// <param name="account">Account for which to get operations</param>
        /// <returns>
        ///     <see cref="OperationsRequestBuilder" />
        /// </returns>
        /// <exception cref="HttpRequestException"></exception>
        public OperationsRequestBuilder ForAccount(KeyPair account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account), "account cannot be null");

            SetSegments("accounts", account.AccountId, "operations");

            return this;
        }

        /// <summary>
        ///     uilds request to GET /ledgers/{ledgerSeq}/operations
        ///     See: https://www.stellar.org/developers/horizon/reference/operations-for-ledger.html
        /// </summary>
        /// <param name="ledgerSeq">Ledger for which to get operations</param>
        /// <returns>
        ///     <see cref="OperationsRequestBuilder" />
        /// </returns>
        public OperationsRequestBuilder ForLedger(long ledgerSeq)
        {
            SetSegments("ledgers", ledgerSeq.ToString(), "operations");

            return this;
        }

        /// <summary>
        ///     Builds request to GET /transactions/{transactionId}/operations
        ///     See: https://www.stellar.org/developers/horizon/reference/operations-for-transaction.html
        /// </summary>
        /// <param name="transactionId">Transaction ID for which to get operations</param>
        /// <returns>
        ///     <see cref="OperationsRequestBuilder" />
        /// </returns>
        public OperationsRequestBuilder ForTransaction(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
                throw new ArgumentNullException(nameof(transactionId), "transactionId cannot be null");

            SetSegments("transactions", transactionId, "operations");

            return this;
        }

        /// <summary>
        ///     Requests specific uri and returns <see cref="Page{OperationResponse}" />.
        ///     This method is helpful for getting the next set of results.
        /// </summary>
        /// <param name="uri">Uri to execute.</param>
        /// <returns>
        ///     <see cref="Page{OperationResponse}" />
        /// </returns>
        /// <exception cref="TooManyRequestsException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<Page<OperationResponse>> Execute(Uri uri)
        {
            var responseHandler = new ResponseHandler<Page<OperationResponse>>();
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(uri);
                return await responseHandler.HandleResponse(response);
            }
        }

        /// <summary>
        ///     Build and execute request.
        /// </summary>
        /// <returns>
        ///     <see cref="Task{Page{OperationResponse}}" />
        /// </returns>
        /// <exception cref="TooManyRequestsException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<Page<OperationResponse>> Execute()
        {
            return await Execute(BuildUri());
        }

        public override RequestBuilder<OperationsRequestBuilder> Cursor(string token)
        {
            base.Cursor(token);
            return this;
        }

        public override RequestBuilder<OperationsRequestBuilder> Limit(int number)
        {
            base.Limit(number);
            return this;
        }

        public override RequestBuilder<OperationsRequestBuilder> Order(OrderDirection direction)
        {
            base.Order(direction);
            return this;
        }
    }
}