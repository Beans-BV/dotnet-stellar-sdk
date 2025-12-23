using System;
using System.Net.Http;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Requests;

public class EffectsRequestBuilder : RequestBuilderStreamable<EffectsRequestBuilder, EffectResponse>
{
    public EffectsRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "effects", httpClient)
    {
    }

    /// <Summary>
    ///     Builds request to <code>GET /accounts/{account}/effects</code>
    ///     <a href="https://www.stellar.org/developers/horizon/reference/effects-for-account.html">Effects for Account</a>
    /// </Summary>
    /// <param name="account">Account for which to get effects</param>
    public EffectsRequestBuilder ForAccount(string account)
    {
        ArgumentException.ThrowIfNullOrEmpty(account);

        if (!StrKey.IsValidEd25519PublicKey(account))
        {
            throw new ArgumentException($"Invalid account ID {account}");
        }
        SetSegments("accounts", account, "effects");
        return this;
    }

    /// <Summary>
    ///     Builds request to <code>GET /ledgers/{ledgerSeq}/effects</code>
    ///     <a href="https://www.stellar.org/developers/horizon/reference/effects-for-ledger.html">Effects for Ledger</a>
    /// </Summary>
    /// <param name="ledgerSeq">Ledger for which to get effects</param>
    public EffectsRequestBuilder ForLedger(long ledgerSeq)
    {
        SetSegments("ledgers", ledgerSeq.ToString(), "effects");
        return this;
    }

    /// <Summary>
    ///     Builds request to <code>GET /transactions/{transactionId}/effects</code>
    ///     <a href="https://www.stellar.org/developers/horizon/reference/effects-for-transaction.html">Effect for Transaction</a>
    /// </Summary>
    /// <param name="transactionId">Transaction ID for which to get effects</param>
    public EffectsRequestBuilder ForTransaction(string transactionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(transactionId);

        SetSegments("transactions", transactionId, "effects");
        return this;
    }

    /// <Summary>
    ///     Builds request to <code>GET /operation/{operationId}/effects</code>
    ///     <a href="https://www.stellar.org/developers/horizon/reference/effects-for-operation.html">Effect for Operation</a>
    /// </Summary>
    /// <param name="operationId">Operation ID for which to get effects</param>
    public EffectsRequestBuilder ForOperation(long operationId)
    {
        SetSegments("operations", operationId.ToString(), "effects");
        return this;
    }

    public EffectsRequestBuilder ForLiquidityPool(LiquidityPoolId liquidityPoolId)
    {
        return ForLiquidityPool(liquidityPoolId.ToString());
    }

    public EffectsRequestBuilder ForLiquidityPool(string liquidityPoolId)
    {
        ArgumentException.ThrowIfNullOrEmpty(liquidityPoolId);

        SetSegments("liquidity_pools", liquidityPoolId, "effects");
        return this;
    }
}