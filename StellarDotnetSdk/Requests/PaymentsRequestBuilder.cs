using System;
using System.Net.Http;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Requests;

public interface IPaymentsRequestInitialBuilder : IPaymentsRequestBuilder
{
    /// <Summary>
    ///     Builds request to <code>GET /accounts/{account}/payments</code>
    ///     <a href="https://developers.stellar.org/network/horizon/resources/get-payments-by-account-id">Payments for Account</a>
    /// </Summary>
    /// <param name="account">Account for which to get payments</param>
    /// <returns>The <see cref="PaymentsRequestBuilder" /> instance.</returns>
    IPaymentsRequestBuilder ForAccount(string account);

    /// <Summary>
    ///     Builds request to <code>GET /ledgers/{ledgerSeq}/payments</code>
    ///     <a href="https://developers.stellar.org/network/horizon/resources/retrieve-a-ledgers-payments">Payments for Ledger</a>
    /// </Summary>
    /// <param name="ledgerSeq">Ledger for which to get payments</param>
    /// <returns>The <see cref="PaymentsRequestBuilder" /> instance.</returns>
    IPaymentsRequestBuilder ForLedger(long ledgerSeq);

    /// <Summary>
    ///     Builds request to <code>GET /transactions/{transactionId}/payments</code>
    ///     Payments for Transaction
    /// </Summary>
    /// <param name="transactionId">Transaction ID for which to get payments</param>
    /// <returns>The <see cref="PaymentsRequestBuilder" /> instance.</returns>
    /// returns>
    IPaymentsRequestBuilder ForTransaction(string transactionId);
}

public interface IPaymentsRequestBuilder : IRequestBuilderStreamable<OperationResponse>,
    IRequestBuilderExecutePageable<IPaymentsRequestBuilder, OperationResponse>
{
    /// <summary>
    ///     Adds a parameter defining whether to include transaction data in the payments.
    /// </summary>
    /// <returns>The <see cref="PaymentsRequestBuilder" /> instance.</returns>
    IPaymentsRequestBuilder IncludeTransaction();
}

/// <summary>
///     Request builder to help build a payments request to fetch payment operations from Horizon.
/// </summary>
public class PaymentsRequestBuilder : RequestBuilderStreamable<IPaymentsRequestBuilder, OperationResponse>,
    IPaymentsRequestInitialBuilder
{
    private PaymentsRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "payments", httpClient)
    {
    }

    /// <inheritdoc />
    public IPaymentsRequestBuilder ForAccount(string account)
    {
        account = account ?? throw new ArgumentNullException(nameof(account), "account cannot be null");
        SetSegments("accounts", account, "payments");
        return this;
    }

    /// <inheritdoc />
    public IPaymentsRequestBuilder ForLedger(long ledgerSeq)
    {
        SetSegments("ledgers", ledgerSeq.ToString(), "payments");
        return this;
    }

    /// <inheritdoc />
    public IPaymentsRequestBuilder ForTransaction(string transactionId)
    {
        transactionId = transactionId ??
                        throw new ArgumentNullException(nameof(transactionId), "transactionId cannot be null");
        SetSegments("transactions", transactionId, "payments");
        return this;
    }

    /// <inheritdoc />
    public IPaymentsRequestBuilder IncludeTransaction()
    {
        UriBuilder.SetQueryParam("join", "transactions");
        return this;
    }

    /// <summary>
    ///     Creates a new <see cref="PaymentsRequestBuilder" /> to build requests to <code>GET /payments</code>
    /// </summary>
    /// <param name="serverUri">The Horizon server URI.</param>
    /// <param name="httpClient">The HttpClient to use for the requests.</param>
    /// <returns>
    ///     The <see cref="IPaymentsRequestInitialBuilder" /> interface for the <see cref="PaymentsRequestBuilder" />
    ///     instance.
    /// </returns>
    public static IPaymentsRequestInitialBuilder Create(Uri serverUri, HttpClient httpClient)
    {
        return new PaymentsRequestBuilder(serverUri, httpClient);
    }
}