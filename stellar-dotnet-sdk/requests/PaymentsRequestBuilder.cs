using stellar_dotnet_sdk.responses.operations;
using System;
using System.Net.Http;

namespace stellar_dotnet_sdk.requests;

public interface IPaymentsRequestInitialBuilder : IPaymentsRequestBuilder
{
    IPaymentsRequestBuilder ForAccount(string account);
    IPaymentsRequestBuilder ForLedger(long ledgerSeq);
    IPaymentsRequestBuilder ForTransaction(string transactionId);
}

public interface IPaymentsRequestBuilder : IRequestBuilderStreamable<OperationResponse>, IRequestBuilderExecutePageable<IPaymentsRequestBuilder, OperationResponse>
{
    IPaymentsRequestBuilder JoinTransactions(bool value);
}

public class PaymentsRequestBuilder : RequestBuilderStreamable<IPaymentsRequestBuilder, OperationResponse>, IPaymentsRequestInitialBuilder
{
    private PaymentsRequestBuilder(Uri serverURI, HttpClient httpClient)
        : base(serverURI, "payments", httpClient)
    {
    }

    /// <summary>
    /// Creates a new <see cref="PaymentsRequestBuilder"/> to build requests to <code>GET /payments</code>
    /// </summary>
    /// <param name="serverURI">The Horizon server URI.</param>
    /// <param name="httpClient">The HttpClient to use for the requests.</param>
    /// <returns>The <see cref="IPaymentsRequestInitialBuilder"/> interface for the <see cref="PaymentsRequestBuilder"/> instance.</returns>
    public static IPaymentsRequestInitialBuilder Create(Uri serverURI, HttpClient httpClient)
    {
        return new PaymentsRequestBuilder(serverURI, httpClient);
    }

    ///<Summary>
    /// Builds request to <code>GET /accounts/{account}/payments</code>
    /// <a href="https://www.stellar.org/developers/horizon/reference/payments-for-account.html">Effects for Account</a>
    /// </Summary>
    /// <param name="account">Account for which to get payments</param>
    /// <returns>The <see cref="PaymentsRequestBuilder"/> instance.</returns>
    public IPaymentsRequestBuilder ForAccount(string account)
    {
        account = account ?? throw new ArgumentNullException(nameof(account), "account cannot be null");
        SetSegments("accounts", account, "payments");
        return this;
    }

    ///<Summary>
    /// Builds request to <code>GET /ledgers/{ledgerSeq}/effects</code>
    /// <a href="https://www.stellar.org/developers/horizon/reference/payments-for-ledger.html">Effects for Ledger</a>
    /// </Summary>
    /// <param name="ledgerSeq">Ledger for which to get effects</param> 
    /// <returns>The <see cref="PaymentsRequestBuilder"/> instance.</returns>
    public IPaymentsRequestBuilder ForLedger(long ledgerSeq)
    {
        SetSegments("ledgers", ledgerSeq.ToString(), "payments");
        return this;
    }

    ///<Summary>
    /// Builds request to <code>GET /transactions/{transactionId}/payments</code>
    /// <a href="https://www.stellar.org/developers/horizon/reference/payments-for-transaction.html">Effect for Transaction</a>
    /// </Summary>
    /// <param name="transactionId">Transaction ID for which to get payments</param>
    /// <returns>The <see cref="PaymentsRequestBuilder"/> instance.</returns>returns>
    public IPaymentsRequestBuilder ForTransaction(string transactionId)
    {
        transactionId = transactionId ?? throw new ArgumentNullException(nameof(transactionId), "transactionId cannot be null");
        SetSegments("transactions", transactionId, "payments");
        return this;
    }
    
    /// <summary>
    /// Adds a parameter defining whether to attach transaction data in the payments.
    /// </summary>
    /// <param name="value">Set to true to include transaction data in the payments.</param>
    /// <returns>The <see cref="PaymentsRequestBuilder"/> instance.</returns>
    public IPaymentsRequestBuilder JoinTransactions(bool value)
    {
        if (value)
        {
            UriBuilder.SetQueryParam("join", "transactions");
        }
        else
        {
            UriBuilder.RemoveQueryParam("join");
        }
        return this;
    }
}
