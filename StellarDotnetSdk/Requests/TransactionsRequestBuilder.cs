using System;
using System.Net.Http;
using System.Threading.Tasks;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

public class TransactionsRequestBuilder : RequestBuilderStreamable<TransactionsRequestBuilder, TransactionResponse>
{
    public TransactionsRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "transactions", httpClient)
    {
    }

    /// <summary>
    ///     Requests specific uri and returns LedgerResponse.
    ///     This method is helpful for getting the links.
    /// </summary>
    public async Task<TransactionResponse> Transaction(Uri uri)
    {
        var responseHandler = new ResponseHandler<TransactionResponse>();

        var response = await HttpClient.GetAsync(uri);
        return await responseHandler.HandleResponse(response);
    }

    /// <summary>
    ///     Requests <code>GET /transactions/{transactionId}</code>
    ///     <a href="https://developers.stellar.org/network/horizon/resources/retrieve-a-transaction">Retrieve a Transaction</a>
    /// </summary>
    /// <param name="transactionId">ID of the transaction to fetch.</param>
    public Task<TransactionResponse> Transaction(string transactionId)
    {
        SetSegments("transactions", transactionId);
        return Transaction(BuildUri());
    }

    /// <Summary>
    ///     Builds request to <code>GET /accounts/{account}/transactions</code>
    ///     <a href="https://developers.stellar.org/network/horizon/resources/get-transactions-by-account-id">
    ///         Retrieve an
    ///         Account's Transactions
    ///     </a>
    /// </Summary>
    /// <param name="account">Account for which to get transactions</param>
    public TransactionsRequestBuilder ForAccount(string account)
    {
        ArgumentException.ThrowIfNullOrEmpty(account);

        if (!StrKey.IsValidEd25519PublicKey(account))
        {
            throw new ArgumentException($"Invalid account ID {account}");
        }
        SetSegments("accounts", account, "transactions");
        return this;
    }

    /// <summary>
    ///     Builds request to <code>GET /claimable_balances/{claimable_balance_id}/transactions</code>
    ///     See:
    ///     <a href="https://developers.stellar.org/network/horizon/resources/cb-retrieve-related-transactions">
    ///         Retrieve
    ///         related transactions
    ///     </a>
    /// </summary>
    /// <param name="claimableBalance">Claimable Balance for which to get transactions</param>
    public TransactionsRequestBuilder ForClaimableBalance(string claimableBalance)
    {
        if (string.IsNullOrWhiteSpace(claimableBalance))
        {
            throw new ArgumentNullException(nameof(claimableBalance), "claimableBalance cannot be null");
        }

        SetSegments("claimable_balances", claimableBalance, "transactions");

        return this;
    }

    /// <summary>
    ///     Builds request to <code>GET /ledgers/{ledgerSeq}/transactions</code>
    ///     <a href="https://developers.stellar.org/network/horizon/resources/retrieve-a-ledgers-transactions">
    ///         Retrieve a
    ///         Ledger's Transactions
    ///     </a>
    /// </summary>
    /// <param name="ledgerSeq">Ledger for which to get transactions</param>
    public TransactionsRequestBuilder ForLedger(long ledgerSeq)
    {
        SetSegments("ledgers", ledgerSeq.ToString(), "transactions");
        return this;
    }

    /// <summary>
    ///     Set <code>include_failed</code> flag to include failed transactions.
    /// </summary>
    /// <param name="includeFailed">Set to true to include failed transactions in results</param>
    public TransactionsRequestBuilder IncludeFailed(bool includeFailed)
    {
        UriBuilder.SetQueryParam("include_failed", includeFailed.ToString().ToLowerInvariant());
        return this;
    }
}