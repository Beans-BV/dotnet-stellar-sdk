using System;
using StellarDotnetSdk.Xdr;
using LiquidityPoolConstantProductParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolConstantProductParameters;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the body of a constant-product liquidity pool entry, including parameters, reserves, and share counts.
/// </summary>
public class LiquidityPoolConstantProduct : LiquidityPoolEntryBody
{
    private LiquidityPoolConstantProduct(
        LiquidityPoolConstantProductParameters parameters,
        long reserveA,
        long reserveB,
        long totalPoolShares,
        long poolSharesTrustLineCount
    )
    {
        Parameters = parameters;
        ReserveA = reserveA;
        ReserveB = reserveB;
        TotalPoolShares = totalPoolShares;
        PoolSharesTrustLineCount = poolSharesTrustLineCount;
    }

    /// <summary>
    ///     The constant-product pool parameters (asset pair and fee).
    /// </summary>
    public LiquidityPoolConstantProductParameters Parameters { get; }

    /// <summary>
    ///     The amount of asset A held in reserve by this pool, in stroops.
    /// </summary>
    public long ReserveA { get; }

    /// <summary>
    ///     The amount of asset B held in reserve by this pool, in stroops.
    /// </summary>
    public long ReserveB { get; }

    /// <summary>
    ///     The total number of pool share tokens that have been issued.
    /// </summary>
    public long TotalPoolShares { get; }

    /// <summary>
    ///     The number of trust lines that hold shares in this pool.
    /// </summary>
    public long PoolSharesTrustLineCount { get; }

    /// <summary>
    ///     Creates a <see cref="LiquidityPoolConstantProduct" /> from an XDR
    ///     <see cref="LiquidityPoolEntry.LiquidityPoolEntryBody.LiquidityPoolEntryConstantProduct" /> object.
    /// </summary>
    /// <param name="xdrConstantProduct">The XDR constant product object.</param>
    /// <returns>A <see cref="LiquidityPoolConstantProduct" /> instance.</returns>
    public static LiquidityPoolConstantProduct FromXdr(
        LiquidityPoolEntry.LiquidityPoolEntryBody.LiquidityPoolEntryConstantProduct xdrConstantProduct)
    {
        var parameters = LiquidityPoolConstantProductParameters.FromXdr(xdrConstantProduct.Params);
        var reserveA = xdrConstantProduct.ReserveA.InnerValue;
        var reserveB = xdrConstantProduct.ReserveB.InnerValue;
        var totalPoolShares = xdrConstantProduct.TotalPoolShares.InnerValue;
        var poolSharesTrustLineCount = xdrConstantProduct.PoolSharesTrustLineCount.InnerValue;
        return new LiquidityPoolConstantProduct(parameters, reserveA, reserveB, totalPoolShares,
            poolSharesTrustLineCount);
    }

    /// <summary>
    ///     Creates a <see cref="LiquidityPoolConstantProduct" /> from an XDR
    ///     <see cref="LiquidityPoolEntry.LiquidityPoolEntryBody" /> object.
    /// </summary>
    /// <param name="xdrBody">The XDR liquidity pool entry body.</param>
    /// <returns>A <see cref="LiquidityPoolConstantProduct" /> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the body is not a constant-product pool.</exception>
    public static LiquidityPoolConstantProduct FromXdrLiquidityPoolEntryBody(
        LiquidityPoolEntry.LiquidityPoolEntryBody xdrBody)
    {
        if (xdrBody.Discriminant.InnerValue != LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT)
        {
            throw new ArgumentException("Not a LiquidityPoolConstantProduct", nameof(xdrBody));
        }
        return FromXdr(xdrBody.ConstantProduct);
    }
}