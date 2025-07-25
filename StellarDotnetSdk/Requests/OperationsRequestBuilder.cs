﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Requests;

public class OperationsRequestBuilder : RequestBuilderStreamable<OperationsRequestBuilder, OperationResponse>
{
    /// <summary>
    ///     Builds requests connected to operations.
    /// </summary>
    public OperationsRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "operations", httpClient)
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

        var response = await HttpClient.GetAsync(uri);
        return await responseHandler.HandleResponse(response);
    }

    /// <summary>
    ///     Requests GET /operations/{operationId}
    ///     See: https://developers.stellar.org/docs/data/apis/horizon/api-reference/retrieve-an-operation
    /// </summary>
    /// <param name="operationId">Operation to fetch</param>
    public OperationsRequestBuilder Operation(long operationId)
    {
        SetSegments("operations", operationId.ToString());
        return this;
    }

    /// <summary>
    ///     Builds request to GET /accounts/{account}/operations
    ///     See: https://developers.stellar.org/docs/data/apis/horizon/api-reference/get-operations-by-account-id
    /// </summary>
    /// <param name="account">Account for which to get operations</param>
    public OperationsRequestBuilder ForAccount(string account)
    {
        ArgumentException.ThrowIfNullOrEmpty(account);

        if (!StrKey.IsValidEd25519PublicKey(account))
        {
            throw new ArgumentException($"Invalid account ID {account}");
        }
        SetSegments("accounts", account, "operations");

        return this;
    }


    /// <summary>
    ///     Builds request to GET /claimable_balances/{claimable_balance_id}/operations
    ///     See: https://developers.stellar.org/docs/data/apis/horizon/api-reference/cb-retrieve-related-operations
    /// </summary>
    /// <param name="claimableBalanceId">Hex-encoded claimable balance ID (0000...) for which to get operations.</param>
    public OperationsRequestBuilder ForClaimableBalance(string claimableBalanceId)
    {
        ArgumentException.ThrowIfNullOrEmpty(claimableBalanceId);
        if (!StrKey.IsValidClaimableBalanceId(ClaimableBalanceIdUtils.ToBase32String(claimableBalanceId)))
        {
            throw new ArgumentException($"Claimable balance ID {claimableBalanceId} is not valid.");
        }
        SetSegments("claimable_balances", claimableBalanceId, "operations");

        return this;
    }

    /// <summary>
    ///     Builds request to GET /ledgers/{ledgerSeq}/operations
    ///     See: https://developers.stellar.org/docs/data/apis/horizon/api-reference/retrieve-a-ledgers-operations
    /// </summary>
    /// <param name="ledgerSeq">Ledger for which to get operations</param>
    public OperationsRequestBuilder ForLedger(long ledgerSeq)
    {
        SetSegments("ledgers", ledgerSeq.ToString(), "operations");

        return this;
    }

    /// <summary>
    ///     Set <code>include_failed</code> flag to include operations of failed transactions.
    /// </summary>
    /// <param name="includeFailed">Set to true to include operations of failed transactions in results</param>
    public OperationsRequestBuilder IncludeFailed(bool includeFailed)
    {
        UriBuilder.SetQueryParam("include_failed", includeFailed.ToString().ToLowerInvariant());
        return this;
    }

    /// <summary>
    ///     Builds request to GET /transactions/{transactionId}/operations
    ///     See: https://developers.stellar.org/docs/data/apis/horizon/api-reference/retrieve-a-transactions-operations
    /// </summary>
    /// <param name="transactionId">Transaction ID for which to get operations</param>
    public OperationsRequestBuilder ForTransaction(string transactionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(transactionId);

        SetSegments("transactions", transactionId, "operations");

        return this;
    }

    /// <summary>
    ///     Builds request to GET /liquidity_pools/{liquidityPoolId}/operations
    ///     See: https://developers.stellar.org/docs/data/apis/horizon/api-reference/lp-retrieve-related-operations
    /// </summary>
    /// <param name="liquidityPoolId">Liquidity pool ID for which to get operations</param>
    public OperationsRequestBuilder ForLiquidityPool(string liquidityPoolId)
    {
        ArgumentException.ThrowIfNullOrEmpty(liquidityPoolId);

        SetSegments("liquidity_pools", liquidityPoolId, "operations");

        return this;
    }
}